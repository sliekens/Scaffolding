// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.DotNet.Scaffolding.Shared.Spectre;
using Spectre.Console.Cli;

namespace Microsoft.DotNet.Tools.Scaffold.Commands
{
    internal class ExternalCommand : Command<ExternalCommand.Settings>
    {
        public class Settings : DefaultCommandSettings
        {
            [CommandArgument(0, "[Command]")]
            public string CommandName { get; set; } = default!;
        }
        public override int Execute([NotNull] CommandContext context, [NotNull] ExternalCommand.Settings settings)
        {
            Console.WriteLine("Executing external command");
            throw new System.NotImplementedException();
        }
    }
}
