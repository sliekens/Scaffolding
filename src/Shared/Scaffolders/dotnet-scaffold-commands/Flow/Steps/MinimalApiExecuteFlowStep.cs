// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.DotNet.Scaffolding.Shared.Spectre;
using Spectre.Console;
using Spectre.Console.Flow;

namespace Microsoft.DotNet.Tools.Scaffold.Commands.Flow.Steps
{
    internal class MinimalApiExecuteFlowStep : IFlowStep
    {
        public string Id => nameof(MinimalApiExecuteFlowStep);

        public string DisplayName => "Execute Scaffolder";

        public ValueTask ResetAsync(IFlowContext context, CancellationToken cancellationToken)
        {
            return new ValueTask();
        }

        public ValueTask<FlowStepResult> RunAsync(IFlowContext context, CancellationToken cancellationToken)
        {
            AnsiConsole.Status()
               .WithSpinner()
               .Start("Running Minimal API Scaffolder", statusContext =>
               {
                   statusContext.Refresh();
                   Thread.Sleep(2000);
                   AnsiConsole.WriteLine("Created Endpoints Class - Endpoints.cs");
               });

            return new ValueTask<FlowStepResult>(FlowStepResult.Success);
        }

        public ValueTask<FlowStepResult> ValidateUserInputAsync(IFlowContext context, CancellationToken cancellationToken)
        {
            return new ValueTask<FlowStepResult>(FlowStepResult.Failure());
        }
    }
}
