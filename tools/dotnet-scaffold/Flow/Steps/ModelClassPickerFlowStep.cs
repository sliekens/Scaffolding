// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.DotNet.Scaffolding.Shared.Project;
using Microsoft.DotNet.Scaffolding.Shared.Project.Workspaces;
using Microsoft.DotNet.Tools.Scaffold.Commands;
using Microsoft.DotNet.Tools.Scaffold.Flow.Discoveries;
using Spectre.Console;
using Spectre.Console.Flow;

namespace Microsoft.DotNet.Tools.Scaffold.Flow.Steps
{
    internal class ModelClassPickerFlowStep : IFlowStep
    {
        private readonly string _pickerDisplay;
        private readonly ExistingClassProperties? _existingClassProperties;

        public ModelClassPickerFlowStep(string pickerDisplay, ExistingClassProperties? existingClassProperties = null)
        {
            _pickerDisplay = pickerDisplay;
            _existingClassProperties = existingClassProperties;
        }

        public string Id => nameof(ModelClassPickerFlowStep);

        public string DisplayName => _pickerDisplay;

        public ValueTask ResetAsync(IFlowContext context, CancellationToken cancellationToken)
        {
            context.Unset($"{FlowProperties.ModelClassName}-{_pickerDisplay.Replace(" ", "")}");
            context.Unset($"{FlowProperties.ModelClassType}-{_pickerDisplay.Replace(" ", "")}");
            return new ValueTask();
        }

        public ValueTask<FlowStepResult> RunAsync(IFlowContext context, CancellationToken cancellationToken)
        {
            var modelDiscovery = new ModelClassDiscovery(_pickerDisplay);
            var modelType = modelDiscovery.Discover(context);

            if (modelDiscovery.State.IsNavigation())
            {
                return new ValueTask<FlowStepResult>(new FlowStepResult { State = modelDiscovery.State });
            }

            if (modelType is not null)
            {
                SetModelClassProperties(context, modelType.Name, modelType);
                return new ValueTask<FlowStepResult>(FlowStepResult.Success);
            }

            AnsiConsole.WriteLine("No model found");

            return new ValueTask<FlowStepResult>(FlowStepResult.Failure());
        }

        public ValueTask<FlowStepResult> ValidateUserInputAsync(IFlowContext context, CancellationToken cancellationToken)
        {
            var modelClassName = context.GetValue<string>(FlowProperties.ModelClassName);
            var projectWorkspace = context.GetValue<RoslynWorkspace>(FlowProperties.SourceProjectWorkspace);
            if (projectWorkspace is null)
            {
                //throw a fit
            }

            if (string.IsNullOrWhiteSpace(modelClassName))
            {
                var command = context.GetValue<Command>(FlowProperties.ScaffolderCommand);
                var parsedCommand = context.GetValue<ParseResult>(FlowProperties.ScaffolderCommandParseResult);
                modelClassName = parsedCommand?.GetValueForOptionWithName<string>(command, "model"); 
            }

            if (string.IsNullOrEmpty(modelClassName))
            {
                return new ValueTask<FlowStepResult>(FlowStepResult.Failure("Model class is required"));
            }

            var matchedTypes = projectWorkspace?.GetMatchingTypes(modelClassName);
            if (matchedTypes is null || !matchedTypes.Any())
            {
                return new ValueTask<FlowStepResult>(FlowStepResult.Failure($"{modelClassName} class was not found in the built project context."));
            }

            SetModelClassProperties(context, modelClassName, matchedTypes.First());
            return new ValueTask<FlowStepResult>(FlowStepResult.Success);
        }

        private void SetModelClassProperties(IFlowContext context, string modelClassName, ModelType modelClassType)
        {
            context.Set(new FlowProperty(
                $"{FlowProperties.ModelClassName}-{_pickerDisplay.Replace(" ", "")}",
                modelClassName,
                _pickerDisplay,
                isVisible: true));

            context.Set(new FlowProperty(
                $"{FlowProperties.ModelClassType}-{_pickerDisplay.Replace(" ", "")}",
                modelClassType,
                _pickerDisplay,
                isVisible: false));
        }
    }
}
