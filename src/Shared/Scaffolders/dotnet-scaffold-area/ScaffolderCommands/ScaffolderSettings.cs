// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.DotNet.Scaffolding.Shared.Spectre;
using Spectre.Console.Cli;

namespace Microsoft.DotNet.Tools.Scaffold.Commands.ScaffolderCommands
{
    internal class ScaffolderSettings : DefaultCommandSettings
    {
        [CommandOption("--project-path")]
        public string? ProjectPath { get; set; }
    }
}
