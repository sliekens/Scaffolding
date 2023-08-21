// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.DotNet.Tools.Scaffold.Commands;
using Microsoft.DotNet.Tools.Scaffold.Scaffolders;
using Spectre.Console;
using Spectre.Console.Flow;

namespace Microsoft.DotNet.Tools.Scaffold.Flow.Steps
{
    internal class ScaffolderExecuteFlowStep : IFlowStep
    {
        public string Id => nameof(ScaffolderExecuteFlowStep);

        public string DisplayName => "Execute Scaffolder";

        public ValueTask ResetAsync(IFlowContext context, CancellationToken cancellationToken)
        {
            return new ValueTask();
        }

        public async ValueTask<FlowStepResult> RunAsync(IFlowContext context, CancellationToken cancellationToken)
        {
            var command = context.GetValue<Command>(FlowProperties.ScaffolderCommand);
            if (command is null || string.IsNullOrEmpty(command.Name))
            {
                return FlowStepResult.Failure("Command/Command name should not be null");
            }

            await AnsiConsole.Status().WithSpinner()
                .Start($"Running scaffolder '{command.Name}'", async statusContext =>
                {
                    statusContext.Refresh();
                    IInternalScaffolder internalScaffolder = ScaffolderFactory.CreateInternalScaffolder(command.Name, context);
                    await internalScaffolder.ExecuteAsync();
                });

            return FlowStepResult.Success;
        }

        public async ValueTask<FlowStepResult> ValidateUserInputAsync(IFlowContext context, CancellationToken cancellationToken)
        {
            await RunAsync(context, cancellationToken);
            return FlowStepResult.Success;
        }
    }
}
