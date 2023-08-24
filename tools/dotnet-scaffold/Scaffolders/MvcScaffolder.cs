// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.DotNet.Scaffolding.Shared.Cli.Utils;
using Microsoft.DotNet.Scaffolding.Shared.Project;
using Microsoft.DotNet.Scaffolding.Shared.Project.Workspaces;
using Microsoft.DotNet.Scaffolding.Shared.T4Templating;
using Microsoft.DotNet.Tools.Scaffold.Commands;
using Microsoft.DotNet.Tools.Scaffold.Templating;
using Spectre.Console;
using Spectre.Console.Flow;

namespace Microsoft.DotNet.Tools.Scaffold.Scaffolders
{
    internal class MvcScaffolder : IInternalScaffolder
    {
        private readonly IFlowContext _flowContext;
        private readonly StringBuilder _commandLineString;

        public MvcScaffolder(IFlowContext flowContext)
        {
            _flowContext = flowContext;
            _commandLineString = new StringBuilder($"dotnet scaffold mvc ");
        }

        public async Task<int> ExecuteAsync()
        {
            await Task.Delay(1);
            var projectPath = _flowContext.GetValue<string>(Flow.FlowProperties.SourceProjectPath);
            _commandLineString.Append($"--project-path {projectPath} ");
            ExecuteEmptyScaffolders(_flowContext);
            PrintCommand(_commandLineString.ToString());
            return 0;
        }

        internal void PrintCommand(string fullCommand)
        {
            //var fullCommand = _flowContext.GetValue<string>(Flow.FlowProperties.ScaffolderCommandString);
            var parseResult = _flowContext.GetValue<ParseResult>(Flow.FlowProperties.ScaffolderCommandParseResult);
            var commandString = _flowContext.GetValue<string>(Flow.FlowProperties.ScaffolderCommandString);
            var nonInteractive = parseResult?.Tokens.Any(x => x.Value.Equals("--non-interactive"));
            if (!nonInteractive.GetValueOrDefault())
            {
                AnsiConsole.WriteLine("To execute the command non-interactively, use:");
                AnsiConsole.WriteLine($"`{fullCommand}`");
            }
        }

        internal void ExecuteEmptyScaffolders(IFlowContext flowContext)
        {
            var command = flowContext.GetValue<System.CommandLine.Command>(Flow.FlowProperties.ScaffolderCommand);
            var projectFilePath = flowContext.GetValue<string>(Flow.FlowProperties.SourceProjectPath);
            var projectFolderPath = Path.GetDirectoryName(projectFilePath);
            var controllerName = flowContext.GetValue<string>(Flow.FlowProperties.ControllerName);
            var actionsController = flowContext.GetValue<bool>(Flow.FlowProperties.ActionsController);
            //item name in `dotnet new` for api controller or mvc controller (empty with or without actions)
            var controllerTypeName = "mvccontroller";
            var actionsParameter = actionsController ? "--actions" : string.Empty;
            //arguments for `dotnet new page`
            if (string.IsNullOrEmpty(controllerName) || string.IsNullOrEmpty(projectFolderPath))
            {
                //need a fugging name
                return;
            }

            var additionalArgs = new List<string>()
            {
                controllerTypeName,
                "--name",
                controllerName,
                "--output",
                projectFolderPath,
                actionsParameter,
            };

            DotnetCommands.ExecuteDotnetNew(projectFilePath, additionalArgs, new Scaffolding.Shared.ConsoleLogger());
        }
    }
}
