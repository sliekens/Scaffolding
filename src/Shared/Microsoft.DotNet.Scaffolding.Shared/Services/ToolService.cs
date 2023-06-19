// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.DotNet.Scaffolding.Shared.Helpers;

namespace Microsoft.DotNet.Scaffolding.Shared.Spectre.Services
{
    public interface IToolService
    {
        ToolInfo GetToolInfo(string toolName);
        IList<string> GetAllToolNames();
        IList<ToolInfo> GetAllTools();
        Task<bool> InstallTool(ToolInfo ToolInfo, string toolContentFolder);
        Task<bool> UninstallTool(string toolName);
        string DotnetScaffolderFolder { get; set; }
    }

    public class ToolService : IToolService
    {
        public ToolService(string dotnetScaffoldFolder)
        {
            DotnetScaffolderFolder = dotnetScaffoldFolder;
            _packagesInfo = LoadPackagesJson();
        }

        public ToolInfo GetToolInfo(string toolName)
        {
            return _packagesInfo.TryGetValue(toolName, out var ToolInfoVal) ? ToolInfoVal : null;
        }

        public async Task<bool> InstallTool(ToolInfo toolInfo, string toolContentFolder)
        {
            if (!Directory.Exists(toolContentFolder) ||
                Directory.GetFiles(toolContentFolder).Length <= 0 ||
                !ValidateToolInfo(toolInfo))
            {
                return false;
            }

            if (!_packagesInfo.ContainsKey(toolInfo.ToolName))
            {
                AddPackageFolderContentAsync(toolInfo, toolContentFolder);
                _packagesInfo.Add(toolInfo.ToolName, toolInfo);
                //add to dictionary
                await SavePackageJsonFileAsync();

                return true;
            }

            return false;
        }

        public async Task<bool> UninstallTool(string toolName)
        {
            if (_packagesInfo.Remove(toolName, out var ToolInfo))
            {
                //change package.json
                await SavePackageJsonFileAsync();
                //change packages folder content
                return true;
            }

            //didnt delete anything
            return false;
        }

        public IList<string> GetAllToolNames()
        {
            return _packagesInfo.Keys.ToList();
        }

        public IList<ToolInfo> GetAllTools()
        {
            return _packagesInfo.Values.ToList();
        }

        private void AddPackageFolderContentAsync(ToolInfo ToolInfo, string toolContentFolder)
        {
            var packagesFolder = Path.Combine(DotnetScaffolderFolder, "packages");
            var currentPackageFolder = Path.Combine(packagesFolder, ToolInfo.ToolName, ToolInfo.ToolVersion);

            if (!Directory.Exists(currentPackageFolder))
            {
                Directory.CreateDirectory(currentPackageFolder);
                foreach (string file in Directory.EnumerateFiles(toolContentFolder, "*", SearchOption.AllDirectories))
                {
                    // Create the corresponding directory structure in the destination directory
                    string relativePath = Path.GetRelativePath(toolContentFolder, file);
                    string destinationFilePath = Path.Combine(currentPackageFolder, relativePath);
                    string destinationDirectory = Path.GetDirectoryName(destinationFilePath) ?? string.Empty;

                    Directory.CreateDirectory(destinationDirectory);

                    // Copy the file to the destination directory
                    File.Copy(file, destinationFilePath);
                }
            }
            else
            {
                //throw a fit
            }
        }

        /*        private async Task RemovePackageFolderContentAsync(ToolInfo ToolInfo)
                {

                }*/

        private async Task SavePackageJsonFileAsync()
        {
            try
            {
                string jsonString = JsonSerializer.Serialize(_packagesInfo);
                var packagesJsonFile = Path.Combine(DotnetScaffolderFolder, "packages.json");
                //get the packages folder, should exist but triple check
                //get the package name, package uri,
                if (File.Exists(packagesJsonFile))
                {
                    await File.WriteAllTextAsync(packagesJsonFile, jsonString);
                }
            }
            catch (JsonException)
            {
                //did fuck all, say that
            }


        }

        public static bool ValidateToolInfo(ToolInfo tool)
        {
            // Check if any required properties are null and return false if so
            if (string.IsNullOrEmpty(tool.ToolName)
                || string.IsNullOrEmpty(tool.ToolVersion)
                || string.IsNullOrEmpty(tool.ToolExeName))
            {
                return false;
            }

            // All required properties are not null, so return true
            return true;
        }

        private IDictionary<string, ToolInfo> LoadPackagesJson()
        {
            var packagesJsonFile = Path.Combine(DotnetScaffolderFolder, "packages.json");
            var packagesFolder = Path.Combine(DotnetScaffolderFolder, "packages");

            Directory.CreateDirectory(DotnetScaffolderFolder);

            if (!File.Exists(packagesJsonFile))
            {
                File.Create(packagesJsonFile);
            }

            Directory.CreateDirectory(packagesFolder);

            //if either creation failed, return empty, exit out of the dotnet-scaffold tool.
            if (!File.Exists(packagesJsonFile) || !Directory.Exists(packagesFolder))
            {
                //throw exception
                //return string.Empty;
            }

            var packagesJsonFileContent = File.ReadAllText(packagesJsonFile);
            IDictionary<string, ToolInfo> allPackages;
            try
            {
                allPackages = JsonSerializer.Deserialize<IDictionary<string, ToolInfo>>(packagesJsonFileContent) ?? new Dictionary<string, ToolInfo>();
            }
            catch (JsonException)
            {
                allPackages = new Dictionary<string, ToolInfo>();
            }

            return allPackages;
        }

        public string DotnetScaffolderFolder { get; set; }
        private readonly IDictionary<string, ToolInfo> _packagesInfo;
    }
}
