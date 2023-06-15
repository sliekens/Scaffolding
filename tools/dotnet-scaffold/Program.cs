// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.DotNet.Scaffolding.Shared.Helpers;
using Microsoft.DotNet.Scaffolding.Shared.Spectre.Services;
using Microsoft.DotNet.Tools.Scaffold.Commands;
using Microsoft.DotNet.Tools.Scaffold.Install;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Microsoft.DotNet.Tools.Scaffold
{
    public class Program
    {
        /*
        dotnet scaffold [generator] [-p|--project] [-n|--nuget-package-dir] [-c|--configuration] [-tfm|--target-framework] [-b|--build-base-path] [--no-build]

        This commands supports the following generators :
            Area
            Controller
            Identity
            Razorpage
            View

        e.g: dotnet scaffold area <AreaNameToGenerate>
             dotnet scaffold identity
             dotnet scaffold razorpage
        */

        public static async Task<int> Main(string[] args)
        {
            //var allCommands = LoadCommands(dotnetScaffoldFolder);
            var registrations = new ServiceCollection();
            string dotnetScaffoldFolder = GetDotnetScaffoldFolderPath();
            var toolsService = new ToolService(dotnetScaffoldFolder);
            registrations.AddSingleton<IToolService>(toolsService);
            var allCommands = toolsService.GetAllTools();

            var registrar = new TypeRegistrar(registrations);
            var app = new CommandApp(registrar);

            app.Configure(config =>
            {
                config.AddCommand<InstallCommand>("install");
                config.AddCommand<UninstallCommand>("uninstall");
                // Add all the ExternalCommands to the CommandApp
                foreach (var command in allCommands)
                {
                    config.AddCommand<ExternalCommand>(command.ToolName);
                }
            });

            args = ValidateArgs(args, allCommands);
            
            await app.RunAsync(args);

            return 0;
        }

        private static string[] ValidateArgs(string[] args, IList<ToolInfo> allCommands)
        {
            List<string> argsList = args.ToList();
            List<string> commandNames = allCommands.Select(x => x.ToolName).ToList();
            commandNames.Add("install");
            commandNames.Add("uninstall");
            if (argsList.Count == 0)
            {
                var commandName = AnsiConsole.Prompt(
                   new SelectionPrompt<string>()
                       .Title("Pick a scaffold command")
                       .PageSize(15)
                       .AddChoices(commandNames));

                argsList.Add(commandName);
            }

            return argsList.ToArray();
        }

        private static string GetDotnetScaffoldFolderPath()
        {
            //if user folder is not found, return empty, exit out of the dotnet-scaffold tool.
            var userProfileFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (string.IsNullOrEmpty(userProfileFolder))
            {
                //throw new Exception("User profile folder not found.");
            }

            //check if these files and folders exist, if not create them
            var dotnetScaffoldFolder = Path.Combine(userProfileFolder, ".dotnet-scaffold");
            return dotnetScaffoldFolder;
        }
    }
}
