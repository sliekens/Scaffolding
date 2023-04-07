// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Spectre.Console.Cli;

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

        public static int Main(string[] args)
        {
            var allCommands = LoadCommands();
            var app = new CommandApp();
/*            app.Configure(config =>
            {
                config.AddCommand<>("add");
                config.AddCommand<CommitCommand>("commit");
                config.AddCommand<RebaseCommand>("rebase");
            });*/
            //default commands are going to be { install, uninstall, command }
            //command will have the default scaffolders { area, controller, minimalapi, identity, razorpage, view }
            //find what if its install or uninstall
            //
            return 0;
        }

        private static object LoadCommands()
        {
            throw new NotImplementedException();
        }
    }
}
