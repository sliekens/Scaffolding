// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.DotNet.Tools.Scaffold.Commands;
using Microsoft.DotNet.Tools.Scaffold.Flow.Discoveries;
using Microsoft.DotNet.Tools.Scaffold.Services;
using Spectre.Console.Flow;

namespace Microsoft.DotNet.Tools.Scaffold.Flow.Steps
{
    internal class ScaffolderPickerFlowStep : IFlowStep
    {
        private readonly Command _command;
        private readonly IToolService _toolService;
        public string Id => nameof(ScaffolderPickerFlowStep);
        public string DisplayName => "Scaffolder Name";

        public ScaffolderPickerFlowStep(IToolService toolService, Command command)
        {
            _command = command;
            _toolService = toolService;
        }

        public ValueTask ResetAsync(IFlowContext context, CancellationToken cancellationToken)
        {
            context.Unset(FlowProperties.ScaffolderCommand);
            context.Unset(FlowProperties.ScaffolderCommandName);
            return new ValueTask();
        }

        public ValueTask<FlowStepResult> RunAsync(IFlowContext context, CancellationToken cancellationToken)
        {
            var command = context.GetValue<Command>("Command");

            if (command is null || command.Name.Equals("dotnet-scaffold", StringComparison.OrdinalIgnoreCase))
            {
                var scaffolderDiscovery = new ScaffolderDiscovery(_toolService);
                command = scaffolderDiscovery.Discover(context);
                if (scaffolderDiscovery.State.IsNavigation())
                {
                    return new ValueTask<FlowStepResult>(new FlowStepResult { State = scaffolderDiscovery.State });
                }
            }

            if (command is null)
            {
                return new ValueTask<FlowStepResult>(FlowStepResult.Failure("Scaffolder command should have been, failed"));
            }

            SetScaffolderCommandProperties(context, command);
            var steps = GetSteps(command);
            return new ValueTask<FlowStepResult>(new FlowStepResult { State = FlowStepState.Success, Steps = steps });
        }

        public ValueTask<FlowStepResult> ValidateUserInputAsync(IFlowContext context, CancellationToken cancellationToken)
        {
            var command = context.GetValue<Command>(FlowProperties.ScaffolderCommand) ?? _command;
            //command should not be null here
            if (command is null || command.Name.Equals("dotnet-scaffold", StringComparison.OrdinalIgnoreCase))
            {
                return new ValueTask<FlowStepResult>(FlowStepResult.Failure("Specific scaffolder command is required!"));
            }

            SetScaffolderCommandProperties(context, command);
            var steps = GetSteps(command);

            return new ValueTask<FlowStepResult>(new FlowStepResult { State = FlowStepState.Success, Steps = steps});
        }

        private IEnumerable<IFlowStep>? GetSteps(Command? command)
        {
            IEnumerable<IFlowStep>? stepDefinitions = null;
            if (command != null &&
                DefaultCommands.DefaultCommandStepsDict.TryGetValue(command.Name, out var steps))
            {
                stepDefinitions = steps;
            }

            return stepDefinitions;
        }

        private void SetScaffolderCommandProperties(IFlowContext context, Command command)
        {
            context.Set(new FlowProperty(
                FlowProperties.ScaffolderCommand,
                command,
                "Scaffolder Command",
                isVisible: false));

            context.Set(new FlowProperty(
                FlowProperties.ScaffolderCommandName,
                command.Name,
                "Scaffolder Name",
                isVisible: true));
        }
    }
}
