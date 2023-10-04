// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.DotNet.Tools.Scaffold.Extensions;
using Microsoft.DotNet.Tools.Scaffold.Helpers;
using Microsoft.DotNet.Tools.Scaffold.Services;

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
            //create root command, add all other nested commands 
            var rootCommand = new RootCommand("scaffold");
            rootCommand.SetHandler(async (context) =>
            {
                await HandleDefaultScaffolders(context, rootCommand);
            });

            var allDefaultCommands = SetAllHandlers();
            foreach (var defaultCommand in allDefaultCommands)
            {
                rootCommand.AddCommand(defaultCommand);
            }

            //var allCommands = rootCommand.Union(rootCommand.Subcommands);
            /*                ;*/
            return await rootCommand.InvokeAsync(args);
        }

        private static List<Command> SetAllHandlers()
        {
            var allDefaultCommands = DefaultCommands.DefaultCommandsDict.Values.ToList();
            var allCommandsWithSubcommands = allDefaultCommands.GetAllCommandsAndSubcommands();
            foreach (var command in allCommandsWithSubcommands)
            {
                command.SetHandler(async (context) =>
                {
                    await HandleDefaultScaffolders(context, command);
                });
            }

            return allDefaultCommands;
        }

        private async static Task<int> HandleDefaultScaffolders(InvocationContext context, Command command)
        {
            await Task.Delay(0);
            if (context.ParseResult.Tokens.Any(x => x.Value.Equals("-h") || x.Value.Equals("--help")))
            {
                return 0;
            }

            var commandExecutor = new CommandExecutor(command, context.ParseResult, GetDotnetScaffoldFolderPath());
            return await commandExecutor.RunScaffolder();
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
