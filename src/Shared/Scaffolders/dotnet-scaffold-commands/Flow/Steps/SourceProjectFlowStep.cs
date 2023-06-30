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
using Microsoft.DotNet.Tools.Scaffold.Commands.Commands;

namespace Microsoft.DotNet.Tools.Scaffold.Commands.Flow.Steps
{
    internal class SourceProjectFlowStep : IFlowStep
    {
        public string Id => nameof(SourceProjectFlowStep);

        public string DisplayName => "Source Project";

        public ValueTask ResetAsync(IFlowContext context, CancellationToken cancellationToken)
        {
            context.Unset(FlowProperties.SourceProjectPath);
            context.Unset(FlowProperties.SourceProjectContext);
            context.Unset(FlowProperties.SourceProjectWorkspace);
            return new ValueTask();
        }

        public ValueTask<FlowStepResult> RunAsync(IFlowContext context, CancellationToken cancellationToken)
        {
            var settings = context.GetValue<ScaffolderSettings>("CommandSettings");

            var path = settings?.ProjectPath;
            if (string.IsNullOrEmpty(path))
            {
                path = Environment.CurrentDirectory;
            }

            if (!Path.IsPathRooted(path))
            {
                path = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, path.Trim(Path.DirectorySeparatorChar)));
            }

            var discovery = new ProjectFileDiscovery(path);
            var projectPath = discovery.Discover(context, path);
            if (discovery.State.IsNavigation())
            {
                return new ValueTask<FlowStepResult>(new FlowStepResult { State = discovery.State });
            }

            if (projectPath is not null)
            {
                SetSourceProjectProperties(context, projectPath);
                return new ValueTask<FlowStepResult>(FlowStepResult.Success);
            }

            AnsiConsole.WriteLine("No projects found in current directory");
            return new ValueTask<FlowStepResult>(FlowStepResult.Failure());
        }

        public ValueTask<FlowStepResult> ValidateUserInputAsync(IFlowContext context, CancellationToken cancellationToken)
        {
            var projectPath = context.GetValue<string>(FlowProperties.SourceProjectPath);
            if (string.IsNullOrEmpty(projectPath))
            {
                var settings = context.GetValue<ScaffolderSettings>("CommandSettings");
                projectPath = settings?.ProjectPath;
            }

            if (string.IsNullOrEmpty(projectPath))
            {
                return new ValueTask<FlowStepResult>(FlowStepResult.Failure("Project path is required"));
            }

            if (!projectPath.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
            {
                return new ValueTask<FlowStepResult>(FlowStepResult.Failure(string.Format("Project path is invalid {0}", projectPath)));
            }

            if (!Path.IsPathRooted(projectPath))
            {
                projectPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, projectPath.Trim(Path.DirectorySeparatorChar)));
            }

            if (!File.Exists(projectPath))
            {
                return new ValueTask<FlowStepResult>(FlowStepResult.Failure(string.Format("Project path does not exist {0}", projectPath)));
            }

            SetSourceProjectProperties(context, projectPath);

            return new ValueTask<FlowStepResult>(FlowStepResult.Success);
        }

        private void SetSourceProjectProperties(IFlowContext context, string projectPath)
        {
            var projectContext = ScaffolderCommandsHelper.BuildProject(projectPath);
            //var projectWorkspace = new RoslynWorkspace(projectContext);

            context.Set(new FlowProperty(
                FlowProperties.SourceProjectPath,
                projectPath,
                "Source Project",
                isVisible: true));

            context.Set(new FlowProperty(
                FlowProperties.SourceProjectContext,
                projectContext,
                "Source Project Context",
                isVisible: false));

/*            context.Set(new FlowProperty(
                FlowProperties.SourceProjectContext,
                projectWorkspace,
                "Source Project Workspace",
                isVisible: false));*/
        }
    }
}
