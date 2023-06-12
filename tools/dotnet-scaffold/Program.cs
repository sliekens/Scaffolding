// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.DotNet.Scaffolding.Shared.Spectre.Services;
using Microsoft.DotNet.Tools.Scaffold.Commands;
using Microsoft.DotNet.Tools.Scaffold.Install;
using Microsoft.Extensions.DependencyInjection;
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
            System.Diagnostics.Debugger.Launch();
            //var allCommands = LoadCommands(dotnetScaffoldFolder);
            var registrations = new ServiceCollection();
            string dotnetScaffoldFolder = GetDotnetScaffoldFolder();
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

            await app.RunAsync(args);

            return 0;
        }

        private static string GetDotnetScaffoldFolder()
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
