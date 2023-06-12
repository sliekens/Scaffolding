// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Spectre.Console.Cli;

namespace Microsoft.DotNet.Scaffolding.Shared.Spectre
{
    public class DefaultCommandSettings : CommandSettings
    {
        [CommandOption("--interactive")]
        public bool? Interactive { get; set; }

        [CommandOption("--force")]
        public bool Force { get; set; } = default!;

        public bool IsInteractive => Interactive == null || Interactive is true;
    }
}
