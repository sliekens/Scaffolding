// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.DotNet.Scaffolding.Shared.Extensions;
using Microsoft.DotNet.Scaffolding.Shared.Project;
using Microsoft.DotNet.Scaffolding.Shared.Project.Workspaces;
using Microsoft.DotNet.Scaffolding.Shared.ProjectModel;
using Microsoft.DotNet.Tools.Scaffold.Commands.Commands;
using Spectre.Console.Flow;

namespace Microsoft.DotNet.Tools.Scaffold.Commands.Flow.Discoveries
{
    internal class ModelClassDiscovery
    {
        internal ModelClassDiscovery(string displayName, string workingDir)
        {
            _workingDir = workingDir;
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
            IList<ModelType> modelsToUse = _displayName == "Model Class" ? ScaffolderCommandsHelper.ModelClasses : ScaffolderCommandsHelper.DbContextClasses;
            var modelsDict = modelsToUse.ToDictionary(x => x.Name, x => x);
            var modelsNames = modelsToUse.ToDictionary(x => $"{x.Name} {x.FullName.ToSuggestion(withBrackets: true)}", x => x.Name);
            if (modelsNames is null || modelsDict is null)
            {
                return null;
            }
            string? chosenModel = Prompt(context, string.Format("Which {0} do you want to use [highlight](found {1})[/]?", _displayName,modelsDict.Count), modelsNames);
            if (chosenModel is null)
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
        private readonly string _workingDir;
        private readonly string _displayName;
    }

}
