// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.Collections.Generic;
using System.Linq;
using Microsoft.DotNet.Scaffolding.Shared.Project;
using Microsoft.DotNet.Scaffolding.Shared.Project.Workspaces;
using Spectre.Console.Flow;

namespace Microsoft.DotNet.Tools.Scaffold.Flow.Discoveries
{
    internal class ModelClassDiscovery
    {
        internal ModelClassDiscovery(string displayName)
        {
            _displayName = displayName;
        }

        internal string? Prompt(IFlowContext context, string title, IDictionary<string, string> modelClasses)
        {
            var modelDisplayNames = modelClasses.Keys.ToList();
            if (modelDisplayNames.Count == 0)
            {
                return null;
            }

            var prompt = new FlowSelectionPrompt<string>()
                .Title(title)
                .AddChoices(modelDisplayNames.Order(), navigation: context.Navigation);

            var result = prompt.Show();
            State = result.State;
            if (result.Value is not null && modelClasses.TryGetValue(result.Value, out string? modelName))
            {
                return modelName;
            }

            return null;
        }

        internal ModelType? Discover(IFlowContext context)
        {
            var projectWorkspace = context.GetValue<RoslynWorkspace>(FlowProperties.SourceProjectWorkspace);
            var modelsToUse = projectWorkspace.GetAllTypes();
            var modelsDict = modelsToUse.ToDictionary(x => x.Name, x => x);
            var modelsNames = modelsToUse.ToDictionary(x => $"{x.Name} {x.FullName.ToSuggestion(withBrackets: true)}", x => x.Name);
            if (modelsNames is null || modelsDict is null)
            {
                return null;
            }

            string? chosenModel = Prompt(context, string.Format("Which {0} do you want to use [highlight](found {1})[/]?", _displayName, modelsDict.Count), modelsNames);
            if (string.IsNullOrEmpty(chosenModel))
            {
                return null;
            }

            if (modelsDict.TryGetValue(chosenModel, out var modelClass))
            {
                return modelClass;
            }

            return null;
        }

        internal FlowStepState State { get; private set; }
        private readonly string _displayName;
    }
}
