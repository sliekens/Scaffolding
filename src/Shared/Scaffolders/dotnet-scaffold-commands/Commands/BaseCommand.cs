// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.DotNet.Tools.Scaffold.Commands.Services;
using NuGet.Common;
using Spectre.Console.Cli;
using Spectre.Console.Flow;

namespace Microsoft.DotNet.Tools.Scaffold.Commands.Commands
{
    public abstract class BaseCommand<TSettings> : AsyncCommand<TSettings>
        where TSettings : CommandSettings
    {
        protected BaseCommand(IFlowProvider flowProvider)
        {
            FlowProvider = flowProvider;
        }

        protected IFlowProvider FlowProvider { get; }

        protected async ValueTask<int> RunFlowAsync(IEnumerable<IFlowStep> flowSteps, TSettings settings, bool interactive = true)
        {
            var properties = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                { "CommandSettings", settings }
            };

            IFlow? flow = null;
            Exception? exception = null;

            try
            {
                flow = FlowProvider.GetFlow(flowSteps, properties, !interactive);

                return await flow.RunAsync(CancellationToken.None);
            }
            catch (Exception)
            { }

            return exception is not null
                ? throw exception
                : int.MinValue;
        }
    }

}
