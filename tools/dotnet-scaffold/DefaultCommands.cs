// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.CommandLine;

namespace Microsoft.DotNet.Tools.Scaffold
{
    internal static class DefaultCommands
    {
        public static List<Command> AllDefaultCommands = new List<Command>()
        {
            GetMinimalApiCommand(), GetAreaCommand(), GetInstallCommand(), GetUninstallCommand()
        };

        public static Command GetMinimalApiCommand()
        {
            Command minimalApiCommand = new("minimalapi");
            return minimalApiCommand.AddSymbols(DefaultCommandOptions.MinimalApiOptions);
        }

        public static Command GetAreaCommand()
        {
            Command areaCommand = new("area");
            return areaCommand.AddSymbols(DefaultCommandOptions.AreaOptions);
        }

        public static Command GetInstallCommand()
        {
            Command installCommand = new("install");
            return installCommand.AddSymbols(DefaultCommandOptions.InstallOptions);
        }

        public static Command GetUninstallCommand()
        {
            Command uninstallCommand = new("uninstall");
            return uninstallCommand.AddSymbols(DefaultCommandOptions.UninstallOptions);
        }

        //cast 'Symbol' into 'Option's and  'Argument's.
        private static Command AddSymbols(this Command command, List<Symbol?> symbolList)
        {
            symbolList.ForEach(x =>
            {
                if (x is Option option)
                {
                    command.AddOption(option);
                }
                else if (x is Argument arg)
                {
                    command.AddArgument(arg);
                }
            });

            return command;
        }
    }

    internal static class DefaultCommandOptions
    {
        public static List<Symbol?> DefaultOptions = new()
        {
            NonInteractive,
            Force
        };

        public static List<Symbol?> ScaffolderOptions = new(DefaultOptions)
        {
            ProjectPath,
            ScaffolderName
        };

        public static List<Symbol?> ModelScaffolderOptions = new(ScaffolderOptions)
        {
            Model,
            DbContext,
            DbProvider
        };

        public static List<Symbol?> MinimalApiOptions = new(ModelScaffolderOptions)
        {
            EndpointsClass,
            OpenApi,
            EndpointsNamespace,
            NoTypedResults
        };

        public static List<Symbol?> AreaOptions = new(ScaffolderOptions)
        {
            Name
        };

        public static List<Symbol?> InstallOptions = new(ScaffolderOptions)
        {
            AddSource
        };

        public static List<Symbol?> UninstallOptions = new(ScaffolderOptions)
        {
            Name
        };

        public static Option<string> ProjectPath = new("--project-path");
        public static Argument<string> ScaffolderName = new();
        public static Option<bool> NonInteractive = new("--non-interactive");
        public static Option<string> Name = new("--name");
        public static Option<bool> Force = new("--force");
        public static Option<string> Model = new("--model");
        public static Option<string> DbContext = new("--dbContext");
        public static Option<string> RelativeFolderPath = new("--relativeFolderPath");
        public static Option<string> DbProvider = new("--databaseProvider");
        public static Option<string> EndpointsClass = new("--endpoints");
        public static Option<bool> OpenApi = new("--open");
        public static Option<string> EndpointsNamespace = new("--endpointsNamespace");
        public static Option<bool> NoTypedResults = new("--noTypedResults");
        public static Option<string> AddSource = new("--add-source");
    }
}
