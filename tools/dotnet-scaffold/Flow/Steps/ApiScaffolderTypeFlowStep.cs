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
    internal class ApiScaffolderTypeFlowStep : IFlowStep
    {
        public string Id => nameof(ApiScaffolderTypeFlowStep);
        public string DisplayName => "API Scaffolder Type";

        public ValueTask ResetAsync(IFlowContext context, CancellationToken cancellationToken)
        {
            context.Unset(FlowProperties.ApiScaffolderTemplate);
            return new ValueTask();
        }

        public ValueTask<FlowStepResult> RunAsync(IFlowContext context, CancellationToken cancellationToken)
        {
            var command = context.GetValue<Command>(FlowProperties.ScaffolderCommand);
            if (command is null ||
                (!command.Name.Equals("api", StringComparison.OrdinalIgnoreCase) &&
                !command.Name.Equals("endpoints", StringComparison.OrdinalIgnoreCase) &&
                !command.Name.Equals("controller", StringComparison.OrdinalIgnoreCase)))
            {
                return new ValueTask<FlowStepResult>(FlowStepResult.Failure("Scaffolder command is not valid!, should be 'dotnet scaffold api ...' here"));
            }

            var apiDiscovery = new ApiDiscovery(command.Name);
            var apiCommandTuple = apiDiscovery.Discover(context);

            if (apiDiscovery.State.IsNavigation())
            {
                return new ValueTask<FlowStepResult>(new FlowStepResult { State = apiDiscovery.State });
            }

            if (apiCommandTuple is not null && apiCommandTuple.Item1 is not null && !string.IsNullOrEmpty(apiCommandTuple.Item2))
            {
                SetApiScaffolderTypeProperties(context, apiCommandTuple.Item1, apiCommandTuple.Item2);
                var steps = GetSteps(apiCommandTuple.Item1.Name, apiCommandTuple.Item2);
                return new ValueTask<FlowStepResult>(new FlowStepResult { State = FlowStepState.Success, Steps = steps });
            }

            return new ValueTask<FlowStepResult>(FlowStepResult.Failure("Scaffolder command is not valid!"));
        }

        public ValueTask<FlowStepResult> ValidateUserInputAsync(IFlowContext context, CancellationToken cancellationToken)
        {
            var command = context.GetValue<Command>(FlowProperties.ScaffolderCommand);
            var templateName = context.GetValue<string>(FlowProperties.ApiScaffolderTemplate);

            if (command is null)
            {
                return new ValueTask<FlowStepResult>(FlowStepResult.Failure("Scaffolder command is not valid!"));
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

            if (command.Name.Equals("endpoints", StringComparison.OrdinalIgnoreCase) || command.Name.Equals("controller", StringComparison.OrdinalIgnoreCase))
            {
                var steps = GetSteps(command.Name, templateName);
                return new ValueTask<FlowStepResult>(new FlowStepResult { State = FlowStepState.Success, Steps = steps });
            }

            return new ValueTask<FlowStepResult>(FlowStepResult.Failure("Scaffolder command is not valid!"));
        }

        private IEnumerable<IFlowStep>? GetSteps(string commandName, string? templateName)
        {
            IEnumerable<IFlowStep>? stepDefinitions = default;
            if (!string.IsNullOrEmpty(commandName) && !string.IsNullOrEmpty(templateName)) { }
            {
                if (commandName == "endpoints" && templateName == "no-ef")
                {
                    stepDefinitions = DefaultCommands.EndpointsNoEfSteps;
                }
                else if (commandName == "endpoints" && templateName == "with-ef")
                {
                    stepDefinitions = DefaultCommands.EndpointsWithEfSteps;
                }
                else if (commandName == "controller" && templateName == "empty")
                {
                    stepDefinitions = DefaultCommands.EmptyApiController;
                }
                else if (commandName == "controller" && templateName == "actions")
                {
                    stepDefinitions = DefaultCommands.ActionsApiController;
                }
            }

            return stepDefinitions;
        }

        private void SetApiScaffolderTypeProperties(IFlowContext context, Command specificApiCommand, string apiScaffolderTemplate)
        {
            context.Set(new FlowProperty(
                FlowProperties.ApiScaffolderTemplate,
                apiScaffolderTemplate,
                DisplayName,
                isVisible: true));

            context.Set(new FlowProperty(
                FlowProperties.ScaffolderCommand,
                specificApiCommand,
                isVisible: false));
        }
    }
}
