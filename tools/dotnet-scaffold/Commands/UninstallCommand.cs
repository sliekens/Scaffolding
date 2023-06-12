// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.DotNet.Scaffolding.Shared.Spectre;
using Microsoft.DotNet.Scaffolding.Shared.Spectre.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Microsoft.DotNet.Tools.Scaffold.Install
{
    internal class UninstallCommand : Command<UninstallCommand.Settings>
    {
        public UninstallCommand(IToolService ToolInfoService)
        {
            _toolInfoService = ToolInfoService ?? throw new ArgumentNullException(nameof(ToolInfoService));
        }

        public class Settings : DefaultCommandSettings
        {
            [CommandOption("--name")]
            public string ToolName { get; set; } = default!;
        }

        public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
        {
            if (settings.Interactive == null || settings.Interactive is true)
            {
                ValidateArgs(settings);
            }

            return 0;
        }

        private void ValidateArgs(Settings settings)
        {
            if (string.IsNullOrEmpty(settings.ToolName))
            {
                var allToolNames = _toolInfoService.GetAllToolNames();
                var toolName = AnsiConsole.Prompt(
                   new SelectionPrompt<string>()
                       .Title(":")
                       .PageSize(15)
                       .AddChoices(allToolNames));

                /*                if (!string.IsNullOrEmpty(sourcePromptValue))
                                {
                                    settings.Source = sourcePromptValue;
                                    //no urls supported yet
                                    //https://www.nuget.org/packages/Microsoft.dotnet-scaffold/7.0.0-rc.2.22510.1

                                }
                                else
                                {
                                    AnsiConsole.WriteLine("whatever dude");
                                }*/
            }
        }

        private readonly IToolService _toolInfoService;
    }
}

