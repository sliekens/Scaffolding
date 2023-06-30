// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.


namespace Microsoft.DotNet.Tools.Scaffold.Commands
{
    public class DefaultCommandSettings
    {
        public bool? Interactive { get; set; }

        public bool Force { get; set; } = default!;

        public bool IsInteractive => Interactive == null || Interactive is true;
    }

    public class ScaffolderSettings : DefaultCommandSettings
    {
        public string? ScaffolderName { get; set; }

        public string? ProjectPath { get; set; }
    }

    public class ModelScaffolderSettings : ScaffolderSettings
    {
        //[Description("Model class to use")]
        public string? ModelClass { get; set; }

        //[Description("DbContext class to use")]
        public string? DataContextClass { get; set; }

        //[Description("Specify the relative output folder path from project where the file needs to be generated, if not specified, file will be generated in the project folder")]
        public string? RelativeFolderPath { get; set; }

        //[Description("Database provider to use. Options include 'sqlserver' (default), 'sqlite', 'cosmos', 'postgres'.")]
        public string? DatabaseProviderString { get; set; }

    }
}
