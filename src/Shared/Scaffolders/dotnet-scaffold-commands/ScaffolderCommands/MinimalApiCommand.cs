// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading.Tasks;
using Microsoft.DotNet.Tools.Scaffold.Commands.ScaffolderCommands;
using Microsoft.DotNet.Scaffolding.Shared.Services;
using Spectre.Console.Cli;
using System.Linq;
using System.Diagnostics;

namespace Microsoft.DotNet.Tools.Scaffold.Commands
{
    internal class MinimalApiCommand : AsyncCommand<MinimalApiCommand.Settings>
    {
        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
        {
            Debugger.Launch();
            ScaffolderCommandsHelper.ValidateScaffolderSettings(settings);
            var dependencyGraph = ProjectHelper.GenerateDependencyGraph(settings.ProjectPath);
            var project = dependencyGraph.Projects.First();
            var packagesDeps = project.Dependencies;
            //var files = project.ContentFiles;
            await Task.Delay(1);
            //perform build,
            //validate args
            return 0;
        }

        public class Settings : ScaffolderSettings
        {
            [CommandOption("--endpoints")]
            //[Description("Endpoints class to use. (not file name)")]
            public string? EndpointsClassName { get; set; }

            [CommandOption("--model")]
            //[Description("Model class to use")]
            public string? ModelClass { get; set; }

            [CommandOption("--dataContext")]
            //[Description("DbContext class to use")]
            public string? DataContextClass { get; set; }

            [CommandOption("--relativeFolderPath")]
            //[Description("Specify the relative output folder path from project where the file needs to be generated, if not specified, file will be generated in the project folder")]
            public string? RelativeFolderPath { get; set; }

            [CommandOption("--open")]
            //[Description("Use this option to enable OpenAPI")]
            public bool OpenApi { get; set; }

            [CommandOption("--endpointsNamespace")]
            //[Description("Specify the name of the namespace to use for the generated controller")]
            public string? EndpointsNamespace { get; set; }

            [CommandOption("--databaseProvider")]
            //[Description("Database provider to use. Options include 'sqlserver' (default), 'sqlite', 'cosmos', 'postgres'.")]
            public string? DatabaseProviderString { get; set; }

            [CommandOption("--noTypedResults")]
            //[Description("Flag to not use TypedResults for minimal apis.")]
            public bool NoTypedResults { get; set; }
        }
    }
}
