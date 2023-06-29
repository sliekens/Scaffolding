// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.DotNet.Scaffolding.Shared.Project;
using Microsoft.DotNet.Scaffolding.Shared.ProjectModel;
using Microsoft.DotNet.Scaffolding.Shared.Spectre;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.ProjectModel;
using Spectre.Console;

namespace Microsoft.DotNet.Tools.Scaffold.Commands
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

        internal static IProjectContext BuildProject(string projectPath)
        {
            var name = Path.GetFileNameWithoutExtension(projectPath);
            IProjectContext projectContext = new CommonProjectContext();

            string buildingProjectTitle = $"Building project '{name}'";
            AnsiConsole.Status()
                .WithSpinner()
                .Start(buildingProjectTitle, statusContext =>
                {
                    statusContext.Refresh();
                    //projectContext = new MsBuildProjectContextBuilder(projectPath, "path", "Debug")
                    //    .Build();
                    Thread.Sleep(4000);
                });

            return projectContext;
        }

        internal static List<string> GetProjectFiles(bool throwOnNoneFound = true)
        {
            var csprojFiles = Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "*.csproj", SearchOption.AllDirectories);
            if (!csprojFiles.Any() && throwOnNoneFound)
            {
                //throw a fit about none found, exit mostly
            }

            return csprojFiles.ToList();
        }

        internal static IList<ModelType> DbContextClasses = new List<ModelType>()
        {
            new ModelType() { Name = "DbContext", FullName = "project\\src\\Data\\DbContext.cs" },
            new ModelType() { Name = "DbContext2", FullName = "project\\src\\Data\\DbContext.cs" }
        };

        internal static IList<ModelType> ModelClasses = new List<ModelType>(DbContextClasses)
        {
            new ModelType() { Name = "Car", FullName = "project\\src\\Models\\Car.cs" },
            new ModelType() { Name = "Chicken", FullName = "project\\src\\Models\\Chicken.cs" }
        };

        private static string? GetProjectPathFromFiles(Dictionary<string, string> files)
        {
            var projectFile = AnsiConsole.Prompt(
                       new SelectionPrompt<string>()
                           .Title("Pick a project file")
                           .PageSize(15)
                           .AddChoices(files.Keys));

            return files.GetValueOrDefault(projectFile);
        }
    }
}
