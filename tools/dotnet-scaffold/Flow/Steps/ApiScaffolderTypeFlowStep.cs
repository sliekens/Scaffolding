// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.DotNet.Tools.Scaffold.Extensions;
using Microsoft.DotNet.Tools.Scaffold.Flow.Discoveries;
using Microsoft.DotNet.Tools.Scaffold.Helpers;
using NuGet.Versioning;
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

            var apiDiscovery = new ApiDiscovery();
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

            if (command is null)
            {
                return new ValueTask<FlowStepResult>(FlowStepResult.Failure("Scaffolder command is not valid!, should be 'dotnet scaffold api ...' here"));
            }

            switch (command.Name.ToLower())
            {
                case "api":
                    return new ValueTask<FlowStepResult>(FlowStepResult.Failure("API command is not valid!, missing sub-command, 'endpoints' or 'controller'"));
                case "endpoints":
                case "controller":
                    break;
                default:
                    return new ValueTask<FlowStepResult>(FlowStepResult.Failure("Scaffolder command is not valid!, should be 'dotnet scaffold api ...' here"));
            }

            if (string.IsNullOrEmpty(templateName))
            {
                var parsedCommand = context.GetValue<ParseResult>(FlowProperties.ScaffolderCommandParseResult);
                var templateArgument = parsedCommand?.GetValueForArgumentWithName<string>(command, DefaultCommandOptions.Template.Name);
                if (string.IsNullOrEmpty(templateArgument))
                {
                    return new ValueTask<FlowStepResult>(FlowStepResult.Failure("'template' argument is required!"));
                }

                ApiScaffolderTemplates.TryGetValue(templateArgument, out templateName);
            }

            var steps = GetSteps(templateName);
            if (steps is not null)
            {
                return new ValueTask<FlowStepResult>(new FlowStepResult { State = FlowStepState.Success, Steps = steps });
            }

            return new ValueTask<FlowStepResult>(FlowStepResult.Failure("Could not find any 'api' scaffolders"));
        }

        private IEnumerable<IFlowStep>? GetSteps(string? templateName)
        {
            IList<IFlowStep>? stepDefinitions = null;
            if (!string.IsNullOrEmpty(templateName) &&
                ApiScaffolderSteps.TryGetValue(templateName, out stepDefinitions))
            {
                return stepDefinitions;
            }

            return null;
        }

        internal Dictionary<string, IList<IFlowStep>>? _apiScaffolderSteps;
        internal Dictionary<string, IList<IFlowStep>> ApiScaffolderSteps => _apiScaffolderSteps ??=
            new Dictionary<string, IList<IFlowStep>>()
            {
                { Helpers.Templates.EmptyApiController, DefaultCommands.EmptyControllerSteps },
                { Helpers.Templates.ActionsApiController, DefaultCommands.ActionsController },
                { Helpers.Templates.ApiEndpoints, DefaultCommands.EndpointsNoEfSteps },
                { Helpers.Templates.ApiEndpointsWithEf, DefaultCommands.EndpointsWithEfSteps }
            };

        internal Dictionary<string, string>? _apiScaffolderTemplates;
        internal Dictionary<string, string> ApiScaffolderTemplates => _apiScaffolderTemplates ??=
            new Dictionary<string, string>()
            {
                { "empty", Helpers.Templates.EmptyApiController },
                { "actions", Helpers.Templates.ActionsApiController },
                { "no-ef", Helpers.Templates.ApiEndpoints },
                { "with-ef",  Helpers.Templates.ApiEndpointsWithEf }
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
