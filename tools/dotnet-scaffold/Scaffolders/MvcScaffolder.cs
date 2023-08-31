// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.DotNet.Scaffolding.Shared.Cli.Utils;
using Spectre.Console;
using Spectre.Console.Flow;

namespace Microsoft.DotNet.Tools.Scaffold.Scaffolders
{
    internal class MvcScaffolder : IInternalScaffolder
    {
        private readonly IFlowContext _flowContext;
        private readonly StringBuilder _commandlineString;

        public MvcScaffolder(IFlowContext flowContext)
        {
            _flowContext = flowContext;
            _commandlineString = new StringBuilder($"dotnet scaffold mvc ");
        }

        public async Task<int> ExecuteAsync()
        {
            await Task.Delay(1);
            var projectPath = _flowContext.GetValue<string>(Flow.FlowProperties.SourceProjectPath);
            var mvcTemplate = _flowContext.GetValue<string>(Flow.FlowProperties.MvcScaffolderTemplate);

            if (string.IsNullOrEmpty(projectPath) || string.IsNullOrEmpty(mvcTemplate))
            {
                //throw a fit
                return -1;
            }


            if (mvcTemplate.Contains("MVC Controller", System.StringComparison.OrdinalIgnoreCase))
            {
                ExecuteEmptyScaffolders();
            }
            else if(mvcTemplate.Contains("Area"))
            {
                ExecuteArea();
            }
            
            PrintCommand();
            return 0;
        }

        internal void PrintCommand()
        {
            var parseResult = _flowContext.GetValue<ParseResult>(Flow.FlowProperties.ScaffolderCommandParseResult);
            var commandString = _flowContext.GetValue<string>(Flow.FlowProperties.ScaffolderCommandString);
            var nonInteractive = parseResult?.Tokens.Any(x => x.Value.Equals("--non-interactive"));
            if (!nonInteractive.GetValueOrDefault())
            {
                AnsiConsole.WriteLine("To execute the command non-interactively, use:");
                AnsiConsole.Write(new Markup($"'[springgreen1]{_commandlineString.ToString().Trim()}[/]'"));
            }
        }

        internal int ExecuteEmptyScaffolders()
        {
            _commandlineString.Append("controller ");
            var projectFilePath = _flowContext.GetValue<string>(Flow.FlowProperties.SourceProjectPath);
            var projectFolderPath = Path.GetDirectoryName(projectFilePath);
            var controllerName = _flowContext.GetValue<string>(Flow.FlowProperties.ControllerName);
            var actionsController = _flowContext.GetValue<bool>(Flow.FlowProperties.ActionsController);
            //item name in `dotnet new` for api controller or mvc controller (empty with or without actions)
            var controllerTypeName = "mvccontroller";
            var actionsParameter = actionsController ? "--actions" : string.Empty;
            //arguments for `dotnet new page`
            if (string.IsNullOrEmpty(controllerName) || string.IsNullOrEmpty(projectFolderPath))
            {
                //need a fugging name
                return -1;
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

            _commandlineString.Append($"--project-path {projectFilePath} ");

            return DotnetCommands.ExecuteDotnetNew(projectFilePath, additionalArgs, new Scaffolding.Shared.ConsoleLogger());
        }

        private int ExecuteArea()
        {
            _commandlineString.Append("area ");
            var areaName = _flowContext.GetValue<string>(Flow.FlowProperties.AreaName);
            var projectPath = _flowContext.GetValue<string>(Flow.FlowProperties.SourceProjectPath);
            if (string.IsNullOrWhiteSpace(areaName) || string.IsNullOrWhiteSpace(projectPath))
            {
                return -1;
                //throw exception
            }

            return EnsureFolderLayout(projectPath, areaName);
        }

        private int EnsureFolderLayout(string projectPath, string areaName)
        {
            string? applicationBasePath = Path.GetDirectoryName(projectPath);
            if (string.IsNullOrWhiteSpace(applicationBasePath) || !Directory.Exists(applicationBasePath))
            {
                return -1;
            }

            string? areaBasePath = Path.Combine(applicationBasePath, "Areas");
            if (!Directory.Exists(areaBasePath))
            {
                Directory.CreateDirectory(areaBasePath);
            }

            var areaPath = Path.Combine(areaBasePath, areaName);
            if (!Directory.Exists(areaPath))
            {
                Directory.CreateDirectory(areaPath);
            }

            foreach (var areaFolder in AreaFolders)
            {
                var path = Path.Combine(areaPath, areaFolder);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }

            return 0;
        }


        private static readonly string[] AreaFolders = new string[]
        {
            "Controllers",
            "Models",
            "Data",
            "Views"
        };
    }
}
