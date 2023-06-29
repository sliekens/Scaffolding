// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.DotNet.Scaffolding.Shared.Spectre;
using Spectre.Console.Cli;

namespace Microsoft.DotNet.Tools.Scaffold.Commands
{
    internal class ScaffolderSettings : DefaultCommandSettings
    {
        [CommandOption("--project-path")]
        public string? ProjectPath { get; set; }
    }

    internal class ModelScaffolderSettings : ScaffolderSettings
    {
        [CommandOption("--model")]
        //[Description("Model class to use")]
        public string? ModelClass { get; set; }

        [CommandOption("--dataContext")]
        //[Description("DbContext class to use")]
        public string? DataContextClass { get; set; }

        [CommandOption("--relativeFolderPath")]
        //[Description("Specify the relative output folder path from project where the file needs to be generated, if not specified, file will be generated in the project folder")]
        public string? RelativeFolderPath { get; set; }

        [CommandOption("--databaseProvider")]
        //[Description("Database provider to use. Options include 'sqlserver' (default), 'sqlite', 'cosmos', 'postgres'.")]
        public string? DatabaseProviderString { get; set; }

    }
}
