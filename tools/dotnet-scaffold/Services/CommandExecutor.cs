// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.DotNet.Tools.Scaffold.Flow.Steps;
using Spectre.Console.Flow;

namespace Microsoft.DotNet.Tools.Scaffold.Services
{
    internal class CommandExecutor
    {
        private readonly Command _command;
        private readonly ParseResult _parseResult;
        private readonly IToolService _toolService;
        private readonly IFlowProvider _flowProvider;

        public CommandExecutor(Command command, ParseResult parseResult, string dotnetScaffolderPath)
        {
            _command = command;
            _parseResult = parseResult;
            _flowProvider = new FlowProvider();
            _toolService = new ToolService(dotnetScaffolderPath);
        }

        private async ValueTask<int> RunFlowAsync(IEnumerable<IFlowStep> flowSteps)
        {
            var properties = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                { Flow.FlowProperties.ScaffolderCommand, _command },
                { Flow.FlowProperties.ScaffolderCommandParseResult, _parseResult }
            };

            Exception? exception = null;
            try
            {
                var nonInteractive = _parseResult.Tokens.Any(x => x.Value.Equals("--non-interactive"));
                IFlow? flow = _flowProvider.GetFlow(flowSteps, properties, nonInteractive);
                return await flow.RunAsync(CancellationToken.None);
            }
            catch (Exception)
            { }

            return exception is not null
                ? throw exception
                : int.MinValue;
        }

        public async Task<int> RunScaffolder()
        {
            IEnumerable<IFlowStep> flowSteps = new IFlowStep[]
            {
                new ScaffolderPickerFlowStep(_toolService, _command),
                new ScaffolderExecuteFlowStep()
            };

            return await RunFlowAsync(flowSteps);
        }
    }
}
