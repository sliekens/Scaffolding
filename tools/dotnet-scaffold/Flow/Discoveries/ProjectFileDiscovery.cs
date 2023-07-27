// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.DotNet.Tools.Scaffold.Flow.Discoveries;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.DotNet.Scaffolding.Shared.Extensions;
using Spectre.Console.Flow;

internal class ProjectFileDiscovery
{
    internal ProjectFileDiscovery(string workingDir)
    {
        WorkingDir = workingDir;
    }

    internal string WorkingDir { get; }
    internal FlowStepState State { get; private set; }

    internal string? Prompt(IFlowContext context, string title, IList<string> projectFiles)
    {
        if (projectFiles.Count == 0)
        {
            return null;
        }

        if (projectFiles.Count == 1)
        {
            return projectFiles[0];
        }

        var prompt = new FlowSelectionPrompt<string>()
            .Title(title)
            .Converter(GetProjectDisplayName)
            .AddChoices(projectFiles.OrderBy(x => Path.GetFileNameWithoutExtension(x)), navigation: context.Navigation);

        var result = prompt.Show();

        State = result.State;

        return result.Value;
    }

    internal string? Discover(IFlowContext context, string path)
    {
        if (!Directory.Exists(path))
        {
            return null;
        }

        var projects = Directory.EnumerateFiles(path, "*.csproj", SearchOption.AllDirectories).ToList();

        if (projects.Count == 0)
        {
            return null;
        }

        return Prompt(context, string.Format("Which project do you want to use [highlight](found {0})[/]?", projects.Count), projects);
    }

    private string GetProjectDisplayName(string projectPath)
    {
        var name = Path.GetFileNameWithoutExtension(projectPath);
        var relativePath = projectPath.MakeRelativePath(WorkingDir).ToSuggestion();

        return $"{name} {relativePath.ToSuggestion(withBrackets: true)}";
    }
}
