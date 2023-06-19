// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Internal;
using Spectre.Console;

namespace Microsoft.DotNet.Tools.Scaffold.Commands.ScaffolderCommands
{
    internal static class ScaffolderCommandsHelper
    {
        internal static void ValidateScaffolderSettings(this ScaffolderSettings settings)
        {
            var projectFiles = GetProjectFiles();
            var formattedCsprojFiles = projectFiles.ToDictionary(x => $"{Path.GetFileName(x)} ({x})", y => y);
            var projectFileValue = settings.ProjectPath;

            if (string.IsNullOrWhiteSpace(projectFileValue) && settings.IsInteractive)
            {
                projectFileValue = GetProjectPathFromFiles(formattedCsprojFiles);
            }

            //projectFileValue should have a value
            if (!File.Exists(projectFileValue) && settings.IsInteractive)
            {
                projectFileValue = GetProjectPathFromFiles(formattedCsprojFiles);
            }
            else
            {
                //throw error and quit about project file does not exist
            }

            //projectFileValue should have a valid project path (.csproj file)
            settings.ProjectPath = projectFileValue;
        }

        private static string? GetProjectPathFromFiles(Dictionary<string, string> files)
        {
            var projectFile = AnsiConsole.Prompt(
                       new SelectionPrompt<string>()
                           .Title("Pick a project file")
                           .PageSize(15)
                           .AddChoices(files.Keys));

            return files.GetValueOrDefault(projectFile);
        }

        private static List<string> GetProjectFiles(bool throwOnNoneFound = true)
        {
            var csprojFiles = Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "*.csproj", SearchOption.AllDirectories);
            if (!csprojFiles.Any() && throwOnNoneFound)
            {
                //throw a fit about none found, exit mostly
            }

            return csprojFiles.ToList();
        }
    }
}
