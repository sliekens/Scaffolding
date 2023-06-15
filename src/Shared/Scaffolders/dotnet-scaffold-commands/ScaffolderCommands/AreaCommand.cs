// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.DotNet.Tools.Scaffold.Commands.ScaffolderCommands;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Microsoft.DotNet.Tools.Scaffold.Commands
{
    internal class AreaCommand : Command<AreaCommand.Settings>
    {
        public class Settings : ScaffolderSettings
        {
            [CommandOption("--name")]
            public string Name { get; set; } = default!;
        }

        public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
        {
            Console.WriteLine("SCAFFOLD AREA");
            Console.WriteLine($"project path - {settings.ProjectPath}");
            Console.WriteLine($"area name - {settings.Name}");

            settings.ValidateScaffolderSettings();
            ValidateAreaArgs(settings);

            return EnsureFolderLayout(settings);
        }

        private void ValidateAreaArgs(Settings settings)
        {
            bool validAreaName = false;
            while(!validAreaName)
            {
                string areaName = AnsiConsole.Ask<string>("Provide an area name");
                if (!string.IsNullOrEmpty(areaName))
                {
                    validAreaName = true;
                    settings.Name = areaName;
                }
            }
        }

        private int EnsureFolderLayout(Settings settings)
        {
            string? applicationBasePath = Path.GetDirectoryName(settings.ProjectPath);
            if (string.IsNullOrWhiteSpace(applicationBasePath) || !Directory.Exists(applicationBasePath))
            {
                return -1;
            }

            string? areaBasePath = Path.Combine(applicationBasePath, "Areas");
            if (!Directory.Exists(areaBasePath))
            {
                Directory.CreateDirectory(areaBasePath);
            }

            var areaPath = Path.Combine(areaBasePath, settings.Name);
            if (!Directory.Exists(areaPath))
            {
                Directory.CreateDirectory(areaPath);
            }

            foreach (var areaFolder in AreaFolders)
            {
                var path = Path.Combine(areaPath, areaFolder);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }

            return 0;
        }

        private static readonly string[] AreaFolders = new string[]
        {
            "Controllers",
            "Models",
            "Data",
            "Views"
        };
    }
}
