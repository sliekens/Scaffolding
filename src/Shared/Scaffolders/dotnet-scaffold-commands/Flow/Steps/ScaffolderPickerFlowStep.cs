// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.DotNet.Tools.Scaffold.Commands.Flow.Discoveries;
using Spectre.Console;
using Spectre.Console.Flow;
using Microsoft.DotNet.Scaffolding.Shared.Project.Workspaces;

namespace Microsoft.DotNet.Tools.Scaffold.Commands.Flow.Steps
{
    internal class ScaffolderPickerFlowStep : IFlowStep
    {
        public string Id => nameof(ScaffolderPickerFlowStep);

        public string DisplayName => "Scaffolder Name";

        public ValueTask ResetAsync(IFlowContext context, CancellationToken cancellationToken)
        {
            context.Unset(FlowProperties.ScaffolderName);
            return new ValueTask();
        }

        public ValueTask<FlowStepResult> RunAsync(IFlowContext context, CancellationToken cancellationToken)
        {
            var settings = context.GetValue<ScaffolderSettings>("CommandSettings");
            var scaffolderName = settings?.ScaffolderName;
            if (string.IsNullOrEmpty(scaffolderName))
            {

            }
            string path = "asfsd";
            var discovery = new ProjectFileDiscovery(path);

            var projectPath = discovery.Discover(context, path);

            if (discovery.State.IsNavigation())
            {
                return new ValueTask<FlowStepResult>(new FlowStepResult { State = discovery.State });
            }

            if (projectPath is not null)
            {
                SetScaffolderProperties(context, projectPath);
                return new ValueTask<FlowStepResult>(FlowStepResult.Success);
            }

            AnsiConsole.WriteLine("No projects found in current directory");

            return new ValueTask<FlowStepResult>(FlowStepResult.Failure());
        }

        public ValueTask<FlowStepResult> ValidateUserInputAsync(IFlowContext context, CancellationToken cancellationToken)
        {
            var scaffolderName = context.GetValue<string>(FlowProperties.ScaffolderName);
            if (string.IsNullOrEmpty(scaffolderName))
            {
                var settings = context.GetValue<ScaffolderSettings>("CommandSettings");
                scaffolderName = settings?.ScaffolderName;
            }
           
            if (string.IsNullOrEmpty(scaffolderName))
            {
                return new ValueTask<FlowStepResult>(FlowStepResult.Failure("Scaffolder name is required"));
            }

            SetScaffolderProperties(context, scaffolderName);

            return new ValueTask<FlowStepResult>(FlowStepResult.Success);
        }

        private void SetScaffolderProperties(IFlowContext context, string scaffolderName)
        {
            context.Set(new FlowProperty(
                FlowProperties.SourceProjectPath,
                scaffolderName,
                "Scaffolder",
                isVisible: true));
        }
    }
}
