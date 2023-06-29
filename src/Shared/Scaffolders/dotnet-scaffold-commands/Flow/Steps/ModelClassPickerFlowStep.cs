// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.DotNet.Scaffolding.Shared.Project;
using Microsoft.DotNet.Scaffolding.Shared.Project.Workspaces;
using Microsoft.DotNet.Tools.Scaffold.Commands.Flow.Discoveries;
using Spectre.Console;
using Spectre.Console.Flow;

namespace Microsoft.DotNet.Tools.Scaffold.Commands.Flow.Steps
{
    internal class ModelClassPickerFlowStep : IFlowStep
    {
        private readonly string _classTypeFullString;
        private readonly string _classTypeShortString;
        private readonly string _pickerDisplay;

        public ModelClassPickerFlowStep(string pickerDisplay, string classTypeFullString, string classTypeShortString)
        {
            _pickerDisplay = pickerDisplay;
            _classTypeFullString = classTypeFullString;
            _classTypeShortString = classTypeShortString;
        }

        public string Id => nameof(ModelClassPickerFlowStep);

        public string DisplayName => _pickerDisplay;

        public ValueTask ResetAsync(IFlowContext context, CancellationToken cancellationToken)
        {
            context.Unset(FlowProperties.ModelClassDisplay);
            context.Unset(FlowProperties.ModelClassType);
            return new ValueTask();
        }

        public ValueTask<FlowStepResult> RunAsync(IFlowContext context, CancellationToken cancellationToken)
        {
            var projectWorkspace = context.GetValue<RoslynWorkspace>(FlowProperties.SourceProjectWorkspace);
            if (projectWorkspace == null)
            {
                //return new ValueTask<FlowStepResult>(FlowStepResult.Failure());
                //throw a fit
            }

            var path = Environment.CurrentDirectory;

            if (!Path.IsPathRooted(path))
            {
                path = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, path.Trim(Path.DirectorySeparatorChar)));
            }

            var modelDiscovery = new ModelClassDiscovery(_pickerDisplay, path);
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
            throw new NotImplementedException();
        }

        public ValueTask<FlowStepResult> ValidateUserInputAsync(IFlowContext context, CancellationToken cancellationToken)
        {
            var modelClassName = context.GetValue<string>(FlowProperties.ModelClassDisplay);
            var projectWorkspace = context.GetValue<RoslynWorkspace>(FlowProperties.SourceProjectWorkspace);
            if (projectWorkspace is null)
            {
                //throw a fit
            }

            if (string.IsNullOrWhiteSpace(modelClassName))
            {
                var settings = context.GetValue<ModelScaffolderSettings>("CommandSettings");
                modelClassName = settings?.ModelClass;
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
                $"{FlowProperties.ModelClassDisplay}-{_pickerDisplay.Replace(" ", "")}",
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
