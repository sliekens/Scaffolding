// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.DotNet.Scaffolding.ComponentModel;
using Microsoft.DotNet.Scaffolding.Helpers.Services;
using Spectre.Console;
using Spectre.Console.Flow;

namespace Microsoft.DotNet.Tools.Scaffold.Flow.Steps
{
    internal class CommandPickerFlowStep : IFlowStep
    {
        private readonly ILogger _logger;
        private readonly IDotNetToolService _dotnetToolService;
        public CommandPickerFlowStep(ILogger logger, IDotNetToolService dotnetToolService)
        {
            _logger = logger;
            _dotnetToolService = dotnetToolService;
        }

        public string Id => nameof(CommandPickerFlowStep);

        public string DisplayName => "Command Name";

        public ValueTask ResetAsync(IFlowContext context, CancellationToken cancellationToken)
        {
            context.Unset(FlowContextProperties.ComponentName);
            context.Unset(FlowContextProperties.ComponentObj);
            context.Unset(FlowContextProperties.CommandName);
            context.Unset(FlowContextProperties.CommandObj);
            return new ValueTask();
        }

        public ValueTask<FlowStepResult> RunAsync(IFlowContext context, CancellationToken cancellationToken)
        {
            var settings = context.GetCommandSettings();
            var componentName = settings?.ComponentName;
            var commandName = settings?.CommandName;
            var allComponents = new List<DotNetToolInfo>();
            KeyValuePair<string, CommandInfo>? commandInfo = null;
            var dotnetToolComponent = _dotnetToolService.GlobalDotNetTools.FirstOrDefault(x => x.Command.Equals(componentName, StringComparison.OrdinalIgnoreCase));
            CommandDiscovery commandDiscovery = new(_dotnetToolService, dotnetToolComponent);
            commandInfo = commandDiscovery.Discover(context);
            if (commandDiscovery.State.IsNavigation())
            {
                return new ValueTask<FlowStepResult>(new FlowStepResult { State = commandDiscovery.State });
            }

            if (commandInfo is null || !commandInfo.HasValue || commandInfo.Value.Value is null || string.IsNullOrEmpty(commandInfo.Value.Key))
            {
                throw new Exception();
            }
            else
            {
                dotnetToolComponent ??= _dotnetToolService.GetDotNetTool(commandInfo.Value.Key);
                if (dotnetToolComponent != null)
                {
                    SelectComponent(context, dotnetToolComponent);
                }

                SelectCommand(context, commandInfo.Value.Value);
            }

            var commandFirstStep = GetFirstParameterBasedStep(commandInfo.Value.Value);
            if (commandFirstStep is null)
            {
                throw new Exception("asdf");
            }

            return new ValueTask<FlowStepResult>(new FlowStepResult { State = FlowStepState.Success, Steps = new List<ParameterBasedFlowStep> { commandFirstStep } });
        }

        public ValueTask<FlowStepResult> ValidateUserInputAsync(IFlowContext context, CancellationToken cancellationToken)
        {
            var settings = context.GetCommandSettings();
            var componentName = settings?.ComponentName;
            var commandName = settings?.CommandName;
            CommandInfo? commandInfo = null;
            var dotnetToolComponent = _dotnetToolService.GlobalDotNetTools.FirstOrDefault(x => x.Command.Equals(componentName, StringComparison.OrdinalIgnoreCase));
            if (dotnetToolComponent != null)
            {
                var allCommands = _dotnetToolService.GetCommands(dotnetToolComponent.Command);
                commandInfo = allCommands.FirstOrDefault(x => x.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                return new ValueTask<FlowStepResult>(FlowStepResult.Failure("No component (dotnet tool) provided!"));
            }

            if (commandInfo is null)
            {
                return new ValueTask<FlowStepResult>(FlowStepResult.Failure($"Invalid or empty command provided for component {componentName}"));
            }

            var commandFirstStep = GetFirstParameterBasedStep(commandInfo);
            if (commandFirstStep is null)
            {
                throw new Exception("asdf");
            }

            SelectComponent(context, dotnetToolComponent);
            SelectCommand(context, commandInfo);
            return new ValueTask<FlowStepResult>(new FlowStepResult { State = FlowStepState.Success, Steps = new List<ParameterBasedFlowStep> { commandFirstStep } });
        }

        internal ParameterBasedFlowStep? GetFirstParameterBasedStep(CommandInfo commandInfo)
        {
            ParameterBasedFlowStep? firstParameterStep = null;
            if (commandInfo.Parameters != null && commandInfo.Parameters.Length != 0)
            {
                firstParameterStep = BuildParameterFlowSteps([.. commandInfo.Parameters]);
            }

            return firstParameterStep;
        }

        internal ParameterBasedFlowStep? BuildParameterFlowSteps(List<Parameter> parameters)
        {
            ParameterBasedFlowStep? firstStep = null;
            ParameterBasedFlowStep? previousStep = null;

            foreach (var parameter in parameters)
            {
                var step = new ParameterBasedFlowStep(parameter, null);
                if (firstStep == null)
                {
                    // This is the first step
                    firstStep = step;
                }
                else
                {
                    if (previousStep != null)
                    {
                        // Connect the previous step to this step
                        previousStep.NextStep = step;
                    }
                }

                previousStep = step;
            }

            return firstStep;
        }

        private void SelectCommand(IFlowContext context, CommandInfo command)
        {
            context.Set(new FlowProperty(
                FlowContextProperties.CommandName,
                command.Name,
                "Command Name",
                isVisible: true));

            context.Set(new FlowProperty(
                FlowContextProperties.CommandObj,
                command,
                isVisible: false));
        }

        private void SelectComponent(IFlowContext context, DotNetToolInfo dotnetToolInfo)
        {
            context.Set(new FlowProperty(
                FlowContextProperties.ComponentName,
                dotnetToolInfo.Command,
                "Component Name",
                isVisible: true));

            context.Set(new FlowProperty(
                FlowContextProperties.ComponentObj,
                dotnetToolInfo,
                isVisible: false));
        }
    }
}
