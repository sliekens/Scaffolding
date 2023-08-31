// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CommandLine;
using System.CommandLine.Parsing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.DotNet.Scaffolding.Shared.Project;
using Microsoft.DotNet.Scaffolding.Shared.Project.Workspaces;
using Microsoft.DotNet.Tools.Scaffold.Commands;
using Microsoft.DotNet.Tools.Scaffold.Flow.Discoveries;
using Spectre.Console.Flow;

namespace Microsoft.DotNet.Tools.Scaffold.Flow.Steps
{
    internal class ExistingOrNewClassFlowStep : IFlowStep
    {
        private readonly ExistingClassProperties _existingClassProperties;
        private readonly string _pickerDisplay;
        private readonly Option<string> _option;

        public ExistingOrNewClassFlowStep(
            string pickerDisplay,
            ExistingClassProperties existingClassProperties,
            Option<string> option)
        {
            _pickerDisplay = pickerDisplay;
            _existingClassProperties = existingClassProperties;
            _option = option;
        }

        public string Id => nameof(ExistingOrNewClassFlowStep);

        public string DisplayName => _pickerDisplay;

        public ValueTask ResetAsync(IFlowContext context, CancellationToken cancellationToken)
        {
            context.Unset($"{FlowProperties.ExistingClassName}-{_pickerDisplay.Replace(" ", "")}");
            context.Unset($"{FlowProperties.ExistingClassType}-{_pickerDisplay.Replace(" ", "")}");
            return new ValueTask();
        }

        public ValueTask<FlowStepResult> RunAsync(IFlowContext context, CancellationToken cancellationToken)
        {
            var modelDiscovery = new ExistingOrNewClassDiscovery(_pickerDisplay, _existingClassProperties);
            var modelType = modelDiscovery.Discover(context);

            if (modelDiscovery.State.IsNavigation())
            {
                return new ValueTask<FlowStepResult>(new FlowStepResult { State = modelDiscovery.State });
            }

            if (modelType is not null)
            {
                SetModelClassProperties(context, modelType.Item1, modelType.Item2);
                return new ValueTask<FlowStepResult>(FlowStepResult.Success);
            }

            return new ValueTask<FlowStepResult>(FlowStepResult.Failure());
        }

        public ValueTask<FlowStepResult> ValidateUserInputAsync(IFlowContext context, CancellationToken cancellationToken)
        {
            var modelClassName = context.GetValue<string>($"{FlowProperties.ExistingClassName}-{_pickerDisplay.Replace(" ", "")}");

            if (string.IsNullOrWhiteSpace(modelClassName))
            {
                var command = context.GetValue<Command>(FlowProperties.ScaffolderCommand);
                var parsedCommand = context.GetValue<ParseResult>(FlowProperties.ScaffolderCommandParseResult);
                modelClassName = parsedCommand?.GetValueForOptionWithName<string>(command, _option.Name);
            }

            if (string.IsNullOrEmpty(modelClassName))
            {
                return new ValueTask<FlowStepResult>(FlowStepResult.Failure($"{_pickerDisplay} is required"));
            }

            var projectWorkspace = context.GetValue<RoslynWorkspace>(FlowProperties.SourceProjectWorkspace);
            if (projectWorkspace is null)
            {
                return new ValueTask<FlowStepResult>(FlowStepResult.Failure("Project workspace expected!"));
            }

            var matchedTypes = projectWorkspace?.GetAllTypes(_existingClassProperties);
            SetModelClassProperties(context, modelClassName, matchedTypes?.FirstOrDefault());
            return new ValueTask<FlowStepResult>(FlowStepResult.Success);
        }

        private void SetModelClassProperties(IFlowContext context, string modelClassName, ModelType? modelClassType = null)
        {
            context.Set(new FlowProperty(
                $"{FlowProperties.ExistingClassName}-{_pickerDisplay.Replace(" ", "")}",
                modelClassName,
                _pickerDisplay,
                isVisible: true));

            if (modelClassType != null)
            {
                context.Set(new FlowProperty(
                    $"{FlowProperties.ExistingClassType}-{_pickerDisplay.Replace(" ", "")}",
                    modelClassType,
                    _pickerDisplay,
                    isVisible: false));
            }
        }
    }
}
