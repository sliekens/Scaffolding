// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.DotNet.Tools.Scaffold.Commands;
using Microsoft.DotNet.Tools.Scaffold.Flow.Discoveries;
using Spectre.Console.Flow;

namespace Microsoft.DotNet.Tools.Scaffold.Flow.Steps
{
    internal class RazorPageTypeFlowStep : IFlowStep
    {
        public string Id => nameof(RazorPageTypeFlowStep);
        public string DisplayName => "Razor Page Scaffolder Type";

        public ValueTask ResetAsync(IFlowContext context, CancellationToken cancellationToken)
        {
            context.Unset(FlowProperties.RazorPageScaffolderTemplate);
            return new ValueTask();
        }

        public ValueTask<FlowStepResult> RunAsync(IFlowContext context, CancellationToken cancellationToken)
        {
            var command = context.GetValue<Command>(FlowProperties.ScaffolderCommand);
            if (command is null ||
                (!command.Name.Equals("razorpages", StringComparison.OrdinalIgnoreCase)))
            {
                return new ValueTask<FlowStepResult>(FlowStepResult.Failure("Scaffolder command is not valid!, should be 'dotnet scaffold razorpages ...' here"));
            }

            var razorPageDiscovery = new RazorPageDiscovery();
            var razorPageCommandTuple = razorPageDiscovery.Discover(context);

            if (razorPageDiscovery.State.IsNavigation())
            {
                return new ValueTask<FlowStepResult>(new FlowStepResult { State = razorPageDiscovery.State });
            }

            if (razorPageCommandTuple is not null && razorPageCommandTuple.Item1 is not null && !string.IsNullOrEmpty(razorPageCommandTuple.Item2))
            {
                SetApiScaffolderTypeProperties(context, razorPageCommandTuple.Item1, razorPageCommandTuple.Item2);
                var steps = GetSteps(razorPageCommandTuple.Item2);
                return new ValueTask<FlowStepResult>(new FlowStepResult { State = FlowStepState.Success, Steps = steps });
            }

            return new ValueTask<FlowStepResult>(FlowStepResult.Failure("Scaffolder command is not valid!"));
        }

        public ValueTask<FlowStepResult> ValidateUserInputAsync(IFlowContext context, CancellationToken cancellationToken)
        {
            var command = context.GetValue<Command>(FlowProperties.ScaffolderCommand);
            var templateName = context.GetValue<string>(FlowProperties.RazorPageScaffolderTemplate);

            if (command is null ||
                (!command.Name.Equals("razorpage", StringComparison.OrdinalIgnoreCase)))
            {
                return new ValueTask<FlowStepResult>(FlowStepResult.Failure("Scaffolder command is not valid!, should be 'dotnet scaffold razorpage ...' here"));
            }

            if (string.IsNullOrEmpty(templateName))
            {
                var parsedCommand = context.GetValue<ParseResult>(FlowProperties.ScaffolderCommandParseResult);
                templateName = parsedCommand?.GetValueForArgumentWithName<string>(command, DefaultCommandOptions.Template.Name);
                if (string.IsNullOrEmpty(templateName))
                {
                    return new ValueTask<FlowStepResult>(FlowStepResult.Failure("'template' argument is required!"));
                }
            }

            var steps = GetSteps(templateName);
            return new ValueTask<FlowStepResult>(new FlowStepResult { State = FlowStepState.Success, Steps = steps });
        }

        internal IEnumerable<IFlowStep>? GetSteps(string? templateName)
        {
            IList<IFlowStep>? stepDefinitions = default;
            if (!string.IsNullOrEmpty(templateName))
            {
                RazorPageScaffolderSteps.TryGetValue(templateName, out stepDefinitions);
            }

            return stepDefinitions;
        }

        internal Dictionary<string, IList<IFlowStep>>? _razorPageScaffolderSteps;
        internal Dictionary<string, IList<IFlowStep>> RazorPageScaffolderSteps => _razorPageScaffolderSteps ??=
            new Dictionary<string, IList<IFlowStep>>()
            {
                { "Razor Pages - Empty", DefaultCommands.EmptyRazorPageSteps }
            };

        private void SetApiScaffolderTypeProperties(IFlowContext context, Command specificApiCommand, string razorPageScaffolderTemplate)
        {
            context.Set(new FlowProperty(
                FlowProperties.RazorPageScaffolderTemplate,
                razorPageScaffolderTemplate,
                DisplayName,
                isVisible: true));

            context.Set(new FlowProperty(
                FlowProperties.ScaffolderCommand,
                specificApiCommand,
                isVisible: false));
        }
    }
}
