// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Threading.Tasks;
//using Spectre.Console;

namespace Microsoft.DotNet.Tools.Scaffold
{
    public class Program
    {
        private const string SCAFFOLD_COMMAND = "scaffold";
        private const string AREA_COMMAND = "--area";
        private const string CONTROLLER_COMMAND = "--controller";
        private const string IDENTITY_COMMAND = "--identity";
        private const string RAZORPAGE_COMMAND = "--razorpage";
        private const string VIEW_COMMAND = "--view";
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
            var dotnetScaffoldFolder = InitializeDotnetScaffold();
            if (!string.IsNullOrEmpty(dotnetScaffoldFolder))
            {
                //"Failed to initialize, goodbye"
                return -1;
            }

            /*
            var allCommands = LoadCommands(dotnetScaffoldFolder);

                        var app = new CommandApp();

                        app.Configure(c =>
                        {
                            c.AddBranch("scaffold", scaffold =>
                            {
                                foreach (var command in allCommands)
                                {
                                    //scaffold.AddCommand(command.Key, command.Value);
                                }

                            });
                            *//*                foreach (var command in allCommands)
                                            {
                                                c.AddCommand<>(command.Key);
                                            }*//*
                            c.CaseSensitivity(CaseSensitivity.None);
                        });

                        await app.RunAsync(args);*/

            //default commands are going to be { install, uninstall, command }
            //command will have the default scaffolders { area, controller, minimalapi, identity, razorpage, view }
            //find what if its install or uninstall
            //
            return 0;
        }

        private static string InitializeDotnetScaffold()
        {
            //if user folder is not found, return empty, exit out of the dotnet-scaffold tool.
            var userProfileFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (string.IsNullOrEmpty(userProfileFolder))
            {
                return string.Empty;
            }

            //check if these files and folders exist, if not create them
            var dotnetScaffoldFolder = Path.Combine(userProfileFolder, ".dotnet-scaffold");
            var packagesJsonFile = Path.Combine(dotnetScaffoldFolder, "packages.json");
            var packagesFolder = Path.Combine(dotnetScaffoldFolder, "packages");

            Directory.CreateDirectory(dotnetScaffoldFolder);

            if (!File.Exists(packagesJsonFile))
            {
                File.Create(packagesJsonFile);
            }

            Directory.CreateDirectory(packagesFolder);

            //if either creation failed, return empty, exit out of the dotnet-scaffold tool.
            if (!File.Exists(packagesFolder) || !Directory.Exists(packagesFolder))
            {
                return string.Empty;
            }

            return dotnetScaffoldFolder;
        }

        /*        private static IDictionary<string, Command> LoadCommands(string userScaffoldFolder)
                {
                    IDictionary<string, Type> allCommands = new Dictionary<string, Type>();
                    allCommands.Add("install", typeof(InstallCommand));
                    //userScaffoldFolder folder should exist, already checked in InitializeDotnetScaffold
                    var packagesJsonFile = Path.Combine(userScaffoldFolder, "packages.json");
                    var packagesFolder = Path.Combine(userScaffoldFolder, "packages");
                    throw new NotImplementedException();
                }*/
    }
}
