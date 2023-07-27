// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.DotNet.Tools.Scaffold.Scaffolders;
using Microsoft.DotNet.Tools.Scaffold.Services;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Flow;

namespace Microsoft.DotNet.Tools.Scaffold.Commands
{
    internal class InstallScaffolder : IInternalScaffolder
    {
        public InstallScaffolder(IFlowContext flowContext)
        {
            _flowContext = flowContext;
        }

        public async Task<int> ExecuteAsync()
        {
            await Task.Delay(1);
            //var toolContentFolder = GetToolFolder(settings.Source);
            string toolContentFolder = "sasdfasd";
            var toolPackageInfo = GetToolInfo(toolContentFolder);
            if (toolPackageInfo == null)
            {
                return -1;
            }

            //await Task.Delay(0);
            bool success = true;// await _packageInfoService.InstallTool(toolPackageInfo, toolContentFolder);

            //create directory and copy all its params
            return success ? 0 : -1;
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
            string? templateJsonPath = Directory.EnumerateFiles(toolContentFolder, "template.json", SearchOption.AllDirectories).FirstOrDefault();
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
                {
                    //failed, write to console
                }
            }

            return null;
        }

        private readonly List<string> _installSources = new() { FolderPath, NupkgFilePath };
        private static readonly string FolderPath = "Folder path (on disk)";
        private static readonly string NupkgFilePath = "NuGet file path (.nupkg)";
        //private readonly IToolService _toolService;
        private readonly IFlowContext _flowContext;
    }
}

