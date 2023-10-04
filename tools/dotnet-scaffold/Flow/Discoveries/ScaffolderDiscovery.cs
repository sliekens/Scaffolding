// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using Microsoft.DotNet.Tools.Scaffold.Helpers;
using Microsoft.DotNet.Tools.Scaffold.Services;
using Spectre.Console.Flow;

namespace Microsoft.DotNet.Tools.Scaffold.Flow.Discoveries
{
    internal class ScaffolderDiscovery
    {
        private readonly IToolService _toolService;
        internal FlowStepState State { get; private set; }
        public ScaffolderDiscovery(IToolService toolService)
        {
            _toolService = toolService;
        }

        internal List<Command> GetDefaultCommands()
        {
            return DefaultCommands.DefaultCommandsDict.Values.ToList();
        }

        internal List<Command> GetInstalledCommands()
        {
            var allTools = _toolService.GetAllTools();
            var installedCommands = allTools.Select(x => new Command(x.ToolName, x.ToolDescription)).ToList();
            return installedCommands;
        }

        internal List<Command> GetAllCommands()
        {
            return GetDefaultCommands().Union(GetInstalledCommands()).ToList();
        }

        internal Command? Discover(IFlowContext context)
        {
            //var allCommands = GetAllCommands();
            return Prompt(context, "Pick a scaffolder", DefaultCommands.DefaultCommandsDict);
        }

        internal Command? Prompt(IFlowContext context, string title, IDictionary<string, Command> allCommandsDict)
        {
            if (allCommandsDict.Count == 0)
            {
                return null;
            }

            var prompt = new FlowSelectionPrompt<string>()
                .Title(title)
                .AddChoices(allCommandsDict.Keys.ToList(), navigation: context.Navigation);

            var result = prompt.Show();

            State = result.State;
            if (!string.IsNullOrEmpty(result.Value) && allCommandsDict.TryGetValue(result.Value, out Command? resultCommand))
            {
                return resultCommand;
            }

            return null;
        }
    }
}
