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
                var steps = GetSteps(apiCommandTuple.Item2);
                return new ValueTask<FlowStepResult>(new FlowStepResult { State = FlowStepState.Success, Steps = steps });
            }

            return new ValueTask<FlowStepResult>(FlowStepResult.Failure("Scaffolder command is not valid!"));
        }

        public ValueTask<FlowStepResult> ValidateUserInputAsync(IFlowContext context, CancellationToken cancellationToken)
        {
            var command = context.GetValue<Command>(FlowProperties.ScaffolderCommand);
            var templateName = context.GetValue<string>(FlowProperties.ApiScaffolderTemplate);

            if (command is null ||
                (!command.Name.Equals("api", StringComparison.OrdinalIgnoreCase) &&
                !command.Name.Equals("endpoints", StringComparison.OrdinalIgnoreCase) &&
                !command.Name.Equals("controller", StringComparison.OrdinalIgnoreCase)))
            {
                return new ValueTask<FlowStepResult>(FlowStepResult.Failure("Scaffolder command is not valid!, should be 'dotnet scaffold api ...' here"));
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

        private IEnumerable<IFlowStep>? GetSteps(string? templateName)
        {
            IList<IFlowStep>? stepDefinitions = default;
            if (!string.IsNullOrEmpty(templateName))
            {
                ApiScaffolderSteps.TryGetValue(templateName, out stepDefinitions);
            }

            return stepDefinitions;
        }

        internal Dictionary<string, IList<IFlowStep>>? _apiScaffolderSteps;
        internal Dictionary<string, IList<IFlowStep>> ApiScaffolderSteps => _apiScaffolderSteps ??=
            new Dictionary<string, IList<IFlowStep>>()
            {
                { "API Controller - Empty", DefaultCommands.EmptyControllerSteps },
                { "API Cotroller with read/write actions", DefaultCommands.ActionsController },
                { "API with read/write endpoints", DefaultCommands.EndpointsNoEfSteps },
                { "API with read/write endpoints, using EF", DefaultCommands.EndpointsWithEfSteps }
            };

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
