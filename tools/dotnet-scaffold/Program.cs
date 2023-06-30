// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
            Debugger.Launch();
            var rootCommand = new RootCommand("scaffold");
            var allCommands = GetAllCommands();

            foreach (var command in allCommands)
            {
                command.SetHandler(() =>
                {
                    HandleAllArgs(args);
                });

                rootCommand.AddCommand(command);
            }
            
            rootCommand.SetHandler(() =>
            {
                HandleAllArgs(args);
            });

            return await rootCommand.InvokeAsync(args);
        }

        private static List<Command> GetAllCommands()
        {
            string dotnetScaffoldFolder = GetDotnetScaffoldFolderPath();
            var toolsService = new ToolService(dotnetScaffoldFolder);
            var allTools = toolsService.GetAllTools();
            var installedCommands = allTools.Select(x => new Command(x.ToolName, x.ToolDescription)).ToList();
            installedCommands.AddRange(DefaultCommands.AllDefaultCommands);
            return installedCommands;
        }

        private static void HandleAllArgs(string[] args)
        {
            Console.WriteLine("ferkking lol");
            //invoke dotnet-scaffold-spectre
            //throw new NotImplementedException();
        }
         
/*        private static string[] ValidateArgs(string[] args, IList<ToolInfo> allCommands)
        {
            List<string> argsList = args.ToList();
            List<string> defaultCommands = new List<string> { "area", "controller", "identity", "minimalapi", "razorpage", "view" };
            List<string> commandNames = allCommands.Select(x => x.ToolName).ToList();
            commandNames.AddRange(defaultCommands);
*//*            if (argsList.Count == 0)
            {
                var commandName = AnsiConsole.Prompt(
                   new SelectionPrompt<string>()
                       .Title("\nPick a scaffold command:")
                       .PageSize(15)
                       .AddChoices(commandNames));

                argsList.Add(commandName);
            }*//*

            return argsList.ToArray();
        }*/

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
