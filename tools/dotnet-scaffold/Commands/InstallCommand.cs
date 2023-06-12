// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.DotNet.Scaffolding.Shared.Helpers;
using Microsoft.DotNet.Scaffolding.Shared.Spectre;
using Microsoft.DotNet.Scaffolding.Shared.Spectre.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Microsoft.DotNet.Tools.Scaffold.Install
{
    internal class InstallCommand : AsyncCommand<InstallCommand.Settings>
    {
        public class Settings : DefaultCommandSettings
        {
            [CommandOption("--add-source")]
            public string Source { get; set; } = default!;
        }

        public InstallCommand(IToolService packageInfoService)
        {
            _packageInfoService = packageInfoService ?? throw new ArgumentNullException(nameof(packageInfoService));
        }

        public override async Task<int> ExecuteAsync([NotNull] CommandContext context, [NotNull] Settings settings)
        {
            if (settings.Interactive == null || settings.Interactive is true)
            {
                ValidateArgs(settings);
            }

            var toolContentFolder = GetToolFolder(settings.Source);
            var toolPackageInfo = GetToolInfo(toolContentFolder);
            if (toolPackageInfo == null)
            {
                return -1;
            }

            bool success = await _packageInfoService.InstallTool(toolPackageInfo, toolContentFolder);
            
            //create directory and copy all its params
            return success ? 0 : -1;
        }

        private void ValidateArgs(Settings settings)
        {
            if (string.IsNullOrEmpty(settings.Source))
            {
                var sourcePrompt = $"Provide one of these sources ({string.Join(",", _installSources)}):";
                string sourcePromptValue = AnsiConsole.Ask<string>(sourcePrompt);

                if (!string.IsNullOrEmpty(sourcePromptValue))
                {
                    settings.Source = sourcePromptValue;
                }
                else
                {
                    AnsiConsole.WriteLine("whatever dude");
                }
            }
        }

        internal string GetToolFolder(string toolPath)
        {
            bool isNupkgFile = File.Exists(toolPath) && Path.GetExtension(toolPath).ToLower().Equals(".nupkg", StringComparison.Ordinal);
            bool isFolder = Directory.Exists(toolPath);
            string toolContentPath = string.Empty;

            if (isNupkgFile)
            {
                var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(tempPath);
                ZipFile.ExtractToDirectory(toolPath, tempPath);
                toolContentPath = tempPath;
            }
            else if (isFolder)
            {
                toolContentPath = toolPath;
            }

            return toolContentPath;
        }

        internal ToolInfo? GetToolInfo(string toolContentFolder)
        {
            string templateJsonPath = Path.Combine(toolContentFolder, "template.json");
            //File.Exists on empty templateJsonPath will return false
            if (File.Exists(templateJsonPath))
            {
                var templateJsonContent = File.ReadAllText(templateJsonPath);
                try
                {
                    var packageInfo = JsonSerializer.Deserialize<ToolInfo>(templateJsonContent);
                    if (packageInfo != null)
                    {
                        return packageInfo;
                    }
                }
                catch (JsonException)
                { //failed, write to console
                }
            }

            return null;
        }

        private readonly List<string> _installSources = new List<string>() { FolderPath, NupkgFilePath };
        private static readonly string FolderPath = "Folder path (on disk)";
        private static readonly string NupkgFilePath = "NuGet file path (.nupkg)";
        private IToolService _packageInfoService;
    }
}

