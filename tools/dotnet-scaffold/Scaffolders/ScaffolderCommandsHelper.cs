// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using Microsoft.DotNet.Scaffolding.Shared.Project;
using Microsoft.DotNet.Scaffolding.Shared.ProjectModel;
using Microsoft.DotNet.Tools.Scaffold.Flow.Steps;
using Microsoft.DotNet.Tools.Scaffold.Scaffolders;
using Microsoft.Extensions.ProjectModel;
using Spectre.Console;
using Spectre.Console.Flow;

namespace Microsoft.DotNet.Tools.Scaffold.Commands
{
    internal static class ScaffolderCommandsHelper
    {

        private static string? _scaffoldToolFolder;
        public static string ScaffoldToolFolder => _scaffoldToolFolder ??= GetGlobalFolder();

        internal static IProjectContext BuildProject(string projectPath)
        {
            //install target, override file if needed, easier and low cost regardless.
            var projectDir = Path.GetDirectoryName(projectPath);
            if (!string.IsNullOrEmpty(projectDir) )
            {
                ScaffoldTargetsInstaller.EnsureTargetImported(
                    Path.GetFileName(projectPath),
                    Path.Combine(projectDir, "obj"));
            }

            var name = Path.GetFileNameWithoutExtension(projectPath);
            IProjectContext projectContext = new CommonProjectContext();

            string buildingProjectTitle = $"Building project '{name}'";
            var targetsLocation = ScaffoldTargetsInstaller.GetTargetsLocation();
            AnsiConsole.Status()
                .WithSpinner()
                .Start(buildingProjectTitle, statusContext =>
                {
                    statusContext.Refresh();
                    projectContext = new MsBuildProjectContextBuilder(projectPath, targetsLocation, "Debug").Build();
                });

            return projectContext;
        }

        private static string GetGlobalFolder()
        {
            var entryLocation = Path.GetDirectoryName(typeof(ScaffolderCommandsHelper).Assembly.Location);
            if (!string.IsNullOrEmpty(entryLocation))
            {
                DirectoryInfo? currDirectory = new(entryLocation);
                while (currDirectory != null)
                {
                    DirectoryInfo? contentDirectory = currDirectory.GetDirectories("content").FirstOrDefault();
                    if (contentDirectory != null)
                    {
                        return contentDirectory.FullName;
                    }

                    currDirectory = currDirectory.Parent;
                }
            }

            return string.Empty;
        }
    }

    internal static class Templates
    {
        //"api endpoints" templates
        internal const string MinimalApiEfGenerator = "Templates\\MinimalApi\\MinimalApiEfGenerator.tt";
        internal const string MinimalApiEfNoClassGenerator = "Templates\\MinimalApi\\MinimalApiEfNoClassGenerator.tt";
        internal const string MinimalApiGenerator = "Templates\\MinimalApi\\MinimalApiGenerator.tt";
        internal const string MinimalApiNoClassGenerator = "Templates\\MinimalApi\\MinimalApiNoClassGenerator.tt";
        internal static List<string> MinimalApiTemplates = new() { MinimalApiEfGenerator, MinimalApiEfNoClassGenerator, MinimalApiGenerator, MinimalApiNoClassGenerator };
        //"razorpage" templates
        //"api controller" templates
        //""

        internal static Dictionary<string, List<string>> ScaffoldingT4Templates = new()
        {
            { "endpoints", MinimalApiTemplates }
        };
    }

    internal static class ScaffolderFactory
    {
        internal static IInternalScaffolder CreateInternalScaffolder(string scaffolderName, IFlowContext context)
        {
            return scaffolderName switch
            {
                "mvc" => new MvcScaffolder(context),
                "mvc controller" => new MvcScaffolder(context),
                "mvc area" => new MvcScaffolder(context),
                "api" => new ApiScaffolder(context),
                "api endpoints" => new ApiScaffolder(context),
                "api controller" => new ApiScaffolder(context),
                "razorpages" => new RazorPageScaffolder(context),
                "install" => new InstallScaffolder(context),
                "uninstall" => new UninstallScaffolder(context),
                _ => throw new ArgumentException("Invalid scaffolder name"),
            };
        }
    }

    internal static class DefaultCommands
    {
        public static Dictionary<string, Command> DefaultCommandsDict = new(StringComparer.OrdinalIgnoreCase)
        {
            { "API", ApiCommand },
            { "MVC", MvcCommand },
            { "Razor Pages", RazorPagesCommand },
            { "Install", InstallCommand },
            { "Uninstall", UninstallCommand }
        };

        public static Dictionary<string, IEnumerable<IFlowStep>> DefaultCommandStepsDict = new()
        {
            { "api", new List<IFlowStep>() { new ApiScaffolderTypeFlowStep() }},
            { "api endpoints", new List<IFlowStep>() { new ApiScaffolderTypeFlowStep() }},
            { "api controller", new List<IFlowStep>() { new ApiScaffolderTypeFlowStep() }},
            { "razorpages", new List<IFlowStep>() { new RazorPageTypeFlowStep() } },
            { "mvc", new List<IFlowStep>() { new MvcScaffolderTypeFlowStep() } },
            { "mvc controller", new List<IFlowStep>() { new MvcScaffolderTypeFlowStep() } },
            { "mvc area", new List<IFlowStep>() { new MvcScaffolderTypeFlowStep() } },
            { "install", new List<IFlowStep>() },
            { "uninstall", new List<IFlowStep>() }
        };

        public static List<IFlowStep> EndpointsWithEfSteps = new()
        {
            new SourceProjectFlowStep(),
            new ModelClassPickerFlowStep("Model Class"),
            new ExistingOrNewClassFlowStep("Endpoints Class", new ExistingClassProperties { IsStatic = true }, DefaultCommandOptions.EndpointsClass),
            new ExistingOrNewClassFlowStep("DbContext Class", new ExistingClassProperties { BaseClass = "Microsoft.EntityFrameworkCore.DbContext"}, DefaultCommandOptions.DbContext)
        };

        public static List<IFlowStep> EndpointsNoEfSteps = new()
        {
            new SourceProjectFlowStep(),
            new ModelClassPickerFlowStep("Model Class"),
            new ExistingOrNewClassFlowStep("Endpoints Class", new ExistingClassProperties { IsStatic = true }, DefaultCommandOptions.EndpointsClass),
        };

        public static List<IFlowStep> EmptyControllerSteps = new()
        {
            new SourceProjectFlowStep(noBuild: true),
            new ControllerNameStep(actions: false)
        };

        public static List<IFlowStep> EmptyRazorPageSteps = new()
        {
            new SourceProjectFlowStep(noBuild: true),
            new RazorPageNameStep()
        };

        public static List<IFlowStep> ActionsController = new()
        {
            new SourceProjectFlowStep(noBuild: true),
            new ControllerNameStep(actions: true)
        };

        public static List<IFlowStep> AreaSteps = new()
        {
            new SourceProjectFlowStep(noBuild: true),
            new AreaNameFlowStep()
        };
        
        private static Command? _apiCommand;
        public static Command ApiCommand
        {
            get
            {
                if (_apiCommand == null)
                {
                    _apiCommand = new("api");
                    ApiControllerCommand.Handler = _apiCommand.Handler;
                    ApiEndpointsCommand.Handler = _apiCommand.Handler;
                    _apiCommand.AddCommand(ApiControllerCommand);
                    _apiCommand.AddCommand(ApiEndpointsCommand);
                }
                return _apiCommand;
            }
        }

        private static Command? _mvcCommand;
        public static Command MvcCommand
        {
            get
            {
                if (_mvcCommand == null)
                {
                    _mvcCommand = new Command("mvc");
                    MvcControllerCommand.Handler = _mvcCommand.Handler;
                    AreaCommand.Handler = _mvcCommand.Handler;
                    _mvcCommand.AddCommand(MvcControllerCommand);
                    _mvcCommand.AddCommand(AreaCommand);
                }

                return _mvcCommand;
            }
        }

        private static Command? _razorpagesCommand;
        public static Command RazorPagesCommand
        {
            get
            {
                if (_razorpagesCommand == null)
                {
                    _razorpagesCommand = new Command("razorpages").AddOptions(DefaultCommandOptions.ModelScaffolderOptions);
                    _razorpagesCommand.AddArgument(DefaultCommandOptions.CrudOperation);
                }

                return _razorpagesCommand;
            }
        }

        private static Command? _apiControllerCommand;
        public static Command ApiControllerCommand
        {
            get
            {
                if (_apiControllerCommand == null)
                {
                    _apiControllerCommand = new Command("controller").AddOptions(DefaultCommandOptions.ControllerOptions);
                    _apiControllerCommand.AddArgument(DefaultCommandOptions.Template);
                }

                return _apiControllerCommand;
            }
        }

        private static Command? _mvcControllerCommand;
        public static Command MvcControllerCommand
        {
            get
            {
                _mvcControllerCommand ??= new Command("controller").AddOptions(DefaultCommandOptions.ControllerOptions);
                return _mvcControllerCommand;
            }
        }

        private static Command? _apiEndpointsCommand;
        public static Command ApiEndpointsCommand
        {
            get
            {
                if (_apiEndpointsCommand == null)
                {
                    _apiEndpointsCommand = new Command("endpoints").AddOptions(DefaultCommandOptions.ApiEndpointsOptions);
                    _apiEndpointsCommand.AddArgument(DefaultCommandOptions.Template);
                }

                return _apiEndpointsCommand;
            }
        }

        private static Command? _areaCommand;
        public static Command AreaCommand
        {
            get
            {
                _areaCommand ??= new Command("area").AddOptions(DefaultCommandOptions.AreaOptions);
                return _areaCommand;
            }
        }

        private static Command? _installCommand;
        public static Command InstallCommand
        {
            get
            {
                _installCommand ??= new Command("install").AddOptions(DefaultCommandOptions.InstallOptions);
                return _installCommand;
            }
        }

        private static Command? _uninstallCommand;
        public static Command UninstallCommand
        {
            get
            {
                _uninstallCommand ??= new Command("uninstall").AddOptions(DefaultCommandOptions.UninstallOptions);
                return _uninstallCommand;
            }

        }

        private static Command? _resetCommand;
        public static Command ResetCommand
        {
            get
            {
                _resetCommand ??= new Command("reset")
                {
                    IsHidden = true
                };

                return _resetCommand;
            }
        }

        private static Command AddOptions(this Command command, List<Option> options)
        {
            options.ForEach(command.AddOption);
            return command;
        }
    }

    internal static class DefaultCommandOptions
    {
        public static List<Option> DefaultOptions = new()
        {
            NonInteractive,
            Force
        };

        public static List<Option> ScaffolderOptions = new(DefaultOptions)
        {
            ProjectPath
        };

        public static List<Option> ModelScaffolderOptions = new(ScaffolderOptions)
        {
            Model,
            DbContext,
            DbProvider
        };

        public static List<Option> ControllerOptions = new(ModelScaffolderOptions)
        {
            Name
        };

        public static List<Option> ApiEndpointsOptions = new(ModelScaffolderOptions)
        {
            EndpointsClass,
            OpenApi,
            EndpointsNamespace,
            NoTypedResults
        };

        public static List<Option> AreaOptions = new(ScaffolderOptions)
        {
            Name
        };

        public static List<Option> InstallOptions = new(ScaffolderOptions)
        {
            AddSource
        };

        public static List<Option> UninstallOptions = new(ScaffolderOptions)
        {
            Name
        };

        public static Option<string> ProjectPath => new("--project-path");
        public static Argument<string> ScaffolderName => new();
        public static Option<bool> NonInteractive => new("--non-interactive");
        public static Option<string> Name => new("--name");
        public static Option<bool> Force => new("--force");
        public static Option<string> RelativeFolderPath => new("--relativeFolderPath");
        public static Argument<string> Template = new("template") { Arity = ArgumentArity.ZeroOrMore };
        //model with dbcontext
        public static Option<string> Model => new("--model");
        public static Option<string> DbContext => new("--dbContext");
        public static Option<string> DbProvider => new("--databaseProvider");
        //razor pages
        public static Argument<string> CrudOperation => new();
        //endpoints
        public static Option<string> EndpointsClass => new("--endpoints-class");
        public static Option<bool> OpenApi => new("--open");
        public static Option<string> EndpointsNamespace => new("--endpointsNamespace");
        public static Option<bool> NoTypedResults => new("--noTypedResults");
        public static Option<string> AddSource => new("--add-source");

        internal static string GetDefaultTemplateName()
        {
            return "default";
        }
    }

    internal static class SystemCommandExtensions
    {
        internal static T? GetValueForOptionWithName<T>(this ParseResult parseResult, Command? command, string optionName) where T : class
        {
            T? optionValue = default;
            if (command is null || string.IsNullOrEmpty(optionName))
            {
                return optionValue;
            }
            Option<T>? optionToCheckFor = command?.Options.FirstOrDefault(x => x.Name.Equals(optionName)) as Option<T>;

            if (optionToCheckFor != null)
            {
                optionValue = parseResult?.GetValueForOption(optionToCheckFor);
            }

            return optionValue;
        }

        internal static T? GetValueForArgumentWithName<T>(this ParseResult parseResult, Command? command, string argName) where T : class
        {
            T? argValue = default;
            if (command is null || string.IsNullOrEmpty(argName))
            {
                return argValue;
            }
            Argument<T>? argToCheckFor = command?.Arguments.FirstOrDefault(x => x.Name.Equals(argName)) as Argument<T>;

            if (argToCheckFor != null)
            {
                argValue = parseResult?.GetValueForArgument(argToCheckFor);
            }

            return argValue;
        }

        internal static List<Command> GetAllCommandsAndSubcommands(this List<Command> commands)
        {
            var allCommands = new List<Command>();
            foreach (var command in commands)
            {
                allCommands.Add(command);
                allCommands.AddRange(command.Subcommands.ToList().GetAllCommandsAndSubcommands());
            }
            return allCommands;
        }
    }
}
