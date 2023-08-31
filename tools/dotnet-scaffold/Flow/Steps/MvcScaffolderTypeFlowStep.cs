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
    internal class MvcScaffolderTypeFlowStep : IFlowStep
    {
        public string Id => nameof(MvcScaffolderTypeFlowStep);
        public string DisplayName => "MVC Scaffolder Type";

        public ValueTask ResetAsync(IFlowContext context, CancellationToken cancellationToken)
        {
            context.Unset(FlowProperties.MvcScaffolderTemplate);
            return new ValueTask();
        }

        public ValueTask<FlowStepResult> RunAsync(IFlowContext context, CancellationToken cancellationToken)
        {
            var command = context.GetValue<Command>(FlowProperties.ScaffolderCommand);
            if (command is null ||
                (!command.Name.Equals("mvc", StringComparison.OrdinalIgnoreCase) &&
                !command.Name.Equals("area", StringComparison.OrdinalIgnoreCase) &&
                !command.Name.Equals("controller", StringComparison.OrdinalIgnoreCase)))
            {
                return new ValueTask<FlowStepResult>(FlowStepResult.Failure("Scaffolder command is not valid!, should be 'dotnet scaffold mvc ...' here"));
            }

            var mvcDiscovery = new MvcDiscovery();
            var mvcTemplate = mvcDiscovery.Discover(context);

            if (mvcDiscovery.State.IsNavigation())
            {
                return new ValueTask<FlowStepResult>(new FlowStepResult { State = mvcDiscovery.State });
            }

            if (!string.IsNullOrEmpty(mvcTemplate))
            {
                SetMvcScaffolderTypeProperties(context, mvcTemplate);
                var steps = GetSteps(mvcTemplate);
                return new ValueTask<FlowStepResult>(new FlowStepResult { State = FlowStepState.Success, Steps = steps });
            }

            return new ValueTask<FlowStepResult>(FlowStepResult.Failure("Scaffolder command is not valid!"));
        }

        public ValueTask<FlowStepResult> ValidateUserInputAsync(IFlowContext context, CancellationToken cancellationToken)
        {
            var command = context.GetValue<Command>(FlowProperties.ScaffolderCommand);
            var templateName = context.GetValue<string>(FlowProperties.RazorPageScaffolderTemplate);

            if (command is null ||
                (!command.Name.Equals("mvc", StringComparison.OrdinalIgnoreCase)))
            {
                return new ValueTask<FlowStepResult>(FlowStepResult.Failure("Scaffolder command is not valid!, should be 'dotnet scaffold mvc ...' here"));
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
                MvcScaffolderSteps.TryGetValue(templateName, out stepDefinitions);
            }

            return stepDefinitions;
        }

        internal Dictionary<string, IList<IFlowStep>>? _mvcScaffolderSteps;
        internal Dictionary<string, IList<IFlowStep>> MvcScaffolderSteps => _mvcScaffolderSteps ??=
            new Dictionary<string, IList<IFlowStep>>()
            {
                { "MVC Area", DefaultCommands.AreaSteps },
                { "MVC Controller - Empty", DefaultCommands.EmptyControllerSteps },
                { "MVC Controller with read/write actions", DefaultCommands.ActionsController },
            };

        private void SetMvcScaffolderTypeProperties(IFlowContext context, string mvcScaffolderTemplate)
        {
            context.Set(new FlowProperty(
                FlowProperties.MvcScaffolderTemplate,
                mvcScaffolderTemplate,
                DisplayName,
                isVisible: true));
        }
    }
}
