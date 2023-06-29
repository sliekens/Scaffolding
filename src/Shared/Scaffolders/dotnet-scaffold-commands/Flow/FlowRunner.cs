// Copyright (c) Microsoft Corporation. All rights reserved.

using Spectre.Console.Flow;

namespace Microsoft.DotNet.Tools.Scaffold.Commands.Flow;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Spectre.Console;
using Spectre.Console.Flow.Resources;

/// <summary>
/// Runs operation flow given initial steps and properties. A CLI commands are responsible for
/// initializing flows with starting step(s) and properties containing applicable CLI arguments.
/// </summary>
public class FlowRunner : IFlow
{
    private class FlowStepData
    {
        public FlowStepData(IFlowStep step)
        {
            Step = step;
        }

        public IFlowStep Step { get; private set; }

        public FlowStepResult? Result { get; set; }

        public bool HadUserInput { get; set; }
    }

    private readonly LinkedList<FlowStepData> _steps;
    private readonly bool _nonInteractive;

    public FlowRunner(IEnumerable<IFlowStep> steps, Dictionary<string, object> properties, bool nonInteractive)
    {
        if (steps == null)
        {
            throw new ArgumentNullException(nameof(steps));
        }

        _steps = new LinkedList<FlowStepData>();
        _nonInteractive = nonInteractive;
        Context = new FlowContext(properties);

        AddSteps(steps);
    }

    /// <inheritdoc />
    public FlowContext Context { get; }

    /// <inheritdoc />
    public string? CurrentStepId { get; private set; }

    /// <inheritdoc />
    public bool IsComplete => _steps.Count > 0 && _steps?.Last?.Value.Result is not null;

    /// <inheritdoc />
    public bool ShowBreadcrumbs { get; set; }

    /// <inheritdoc />
    public bool ShowSelectedOptions { get; set; }

    /// <inheritdoc />
    public async ValueTask<int> RunAsync(CancellationToken cancellationToken)
    {
        if (_steps.Count == 0)
        {
            return 0;
        }

        var stepNode = _steps.First!;
        LinkedListNode<FlowStepData>? lastInputStep = null;

        while (stepNode is not null)
        {
            CurrentStepId = stepNode.Value.Step.Id;

            FlowStepResult result;
            try
            {
                result = await stepNode.Value.Step.ValidateUserInputAsync(Context, cancellationToken);
                if (result.State != FlowStepState.Success)
                {
                    if (_nonInteractive && result.State == FlowStepState.Failure)
                    {
                        var message = result.Message ?? string.Format(Strings.UserInputIsRequiredForStep, stepNode.Value.Step.DisplayName);
                        AnsiConsole.WriteLine(message);

                        return -1;
                    }

                    Context.Navigation().Back = lastInputStep is not null;

                    RefreshFlowStatus(Context, stepNode);

                    result = await stepNode.Value.Step.RunAsync(Context, cancellationToken);
                    stepNode.Value.HadUserInput = true;
                }

                stepNode.Value.Result = result;
            }
            catch (Exception ex)
            {
                result = FlowStepResult.Failure(ex.ToString());
            }

            switch (result.State)
            {
                case FlowStepState.Success:
                    AddSteps(result.Steps, stepNode);
                    lastInputStep = stepNode.Value.HadUserInput ? stepNode : lastInputStep;
                    stepNode = stepNode.Next;
                    break;
                case FlowStepState.Back:
                    stepNode = await ResetStepsAsync(Context, stepNode, untilNode: lastInputStep!, cancellationToken);
                    lastInputStep = FindPreviousInputStep(stepNode);
                    break;
                case FlowStepState.Exit:
                    if (ShouldExit())
                    {
                        return -1;
                    }

                    // repeat current step
                    break;
                case FlowStepState.Failure:
                default:
                    if (!string.IsNullOrEmpty(result.Message))
                    {
                        AnsiConsole.WriteLine(result.Message!);
                    }
                    return -1;
            }
        }

        CurrentStepId = null;

        return 0;
    }

    private void AddSteps(IEnumerable<IFlowStep>? steps, LinkedListNode<FlowStepData>? after = null)
    {
        if (steps is null)
        {
            return;
        }

        after ??= _steps.First;

        foreach (var step in steps)
        {
            after = after is null
                ? _steps.AddLast(new FlowStepData(step))
                : _steps.AddAfter(after, new FlowStepData(step));
        }
    }

    private async ValueTask<LinkedListNode<FlowStepData>> ResetStepsAsync(
        FlowContext context,
        LinkedListNode<FlowStepData> currentStepNode,
        LinkedListNode<FlowStepData> untilNode,
        CancellationToken cancellationToken)
    {
        while (currentStepNode != untilNode)
        {
            await ResetStepAsync(context, currentStepNode, cancellationToken);

            currentStepNode = currentStepNode.Previous!;
        }

        await ResetStepAsync(context, currentStepNode, cancellationToken);

        return untilNode;
    }

    private async ValueTask ResetStepAsync(
        FlowContext context,
        LinkedListNode<FlowStepData> currentStepNode,
        CancellationToken cancellationToken)
    {
        if (currentStepNode.Value.Result?.Steps is not null)
        {
            RemoveNextSteps(currentStepNode, currentStepNode.Value.Result.Steps);
        }

        await currentStepNode!.Value.Step.ResetAsync(context, cancellationToken);
    }

    private void RemoveNextSteps(LinkedListNode<FlowStepData> stepNode, IEnumerable<IFlowStep> nextSteps)
    {
        foreach (var nextStep in nextSteps)
        {
            if (stepNode.Next is null)
            {
                return;
            }

            stepNode.List!.Remove(stepNode.Next);
        }
    }

    private LinkedListNode<FlowStepData>? FindPreviousInputStep(LinkedListNode<FlowStepData>? stepNode)
    {
        stepNode = stepNode?.Previous;
        while (stepNode is not null)
        {
            if (stepNode.Value.HadUserInput)
            {
                return stepNode;
            }

            stepNode = stepNode.Previous;
        }

        return null;
    }

    private static bool ShouldExit()
    {
        var prompt = new ConfirmationPrompt(Strings.ExitPrompt)
            .ShowDefaultValue(true);

        prompt.DefaultValue = false;

        return AnsiConsole.Prompt(prompt);
    }

    private void RefreshFlowStatus(FlowContext context, LinkedListNode<FlowStepData> stepNode)
    {
        Console.Clear();

        PrintSelectedOptions(context);

        PrintBreadcrumbs(stepNode);
    }

    private void PrintSelectedOptions(FlowContext context)
    {
        if (!ShowSelectedOptions)
        {
            return;
        }

        var table = new Table()
            .Border(TableBorder.Simple)
            .BorderColor(Color.Grey)
            .AddColumn(new TableColumn(Strings.SelectedOptions.ToHeader()).PadLeft(0))
            .AddColumn(new TableColumn(string.Empty));

        var visibleProperties = context.Properties.GetAll().Where(x => x.IsVisible).OrderBy(x => x.Order).ThenBy(x => x.Name);
        if (visibleProperties.Any())
        {
            foreach (var property in visibleProperties)
            {
                table.AddRow(property!.DisplayName, (property.Value?.ToString() ?? string.Empty).ToSuggestion());
            }
        }
        else
        {
            table.AddRow(Strings.NoOptionsSelected.ToSuggestion(), " ");
        }

        AnsiConsole.Write(table);
    }

    private void PrintBreadcrumbs(LinkedListNode<FlowStepData> currentStepNode)
    {
        if (!ShowBreadcrumbs)
        {
            return;
        }

        if (_steps.Count == 0)
        {
            return;
        }

        var breadcrumbsTable = new Table()
            .Border(TableBorder.Simple)
            .BorderColor(Color.Grey)
            .AddColumn(new TableColumn(Strings.Steps.ToHeader()).Footer(string.Empty).PadLeft(0));

        var builder = new StringBuilder();
        var stepNode = _steps.First!;
        while (stepNode != currentStepNode)
        {
            if (stepNode!.Value.HadUserInput)
            {
                if (stepNode.Previous is not null && stepNode.Previous.Value.HadUserInput)
                {
                    builder.Append(" / ");
                }

                builder.Append(stepNode.Value.Step.DisplayName.ToSuggestion());
            }

            stepNode = stepNode.Next;
        }

        if (builder.Length > 0)
        {
            builder.Append(" / ");
        }

        builder.Append(currentStepNode.Value.Step.DisplayName);

        breadcrumbsTable.AddRow(builder.ToString());

        AnsiConsole.Write(breadcrumbsTable);
    }
}
