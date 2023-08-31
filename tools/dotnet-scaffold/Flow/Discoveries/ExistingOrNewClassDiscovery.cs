// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.DotNet.Scaffolding.Shared.Project;
using Microsoft.DotNet.Scaffolding.Shared.Project.Workspaces;
using Spectre.Console;
using Spectre.Console.Flow;

namespace Microsoft.DotNet.Tools.Scaffold.Flow.Discoveries
{
    internal class ExistingOrNewClassDiscovery
    {
        public ExistingOrNewClassDiscovery(string displayName, ExistingClassProperties classProperties)
        {
            _displayName = displayName;
            _classProperties = classProperties;
        }

        internal Tuple<string, ModelType?>? Discover(IFlowContext context)
        {
            var projectWorkspace = context.GetValue<RoslynWorkspace>(FlowProperties.SourceProjectWorkspace);
            var allTypes = projectWorkspace.GetAllTypes(_classProperties);
            var allTypesDict = allTypes.ToDictionary(x => x.Name, x => x);
            string? chosenTypeName = Prompt(context, string.Format("Which {0} do you want to use [highlight](found {1})[/]?", _displayName, allTypesDict.Count), allTypesDict);

            if (chosenTypeName is null || chosenTypeName.Equals("Create"))
            {
                var prompt = new TextPrompt<string>($"Enter a new [hotpink3_1]{_displayName}[/] (or [lightseagreen]<[/] to go back).")
                    .ValidationErrorMessage($"Not a valid {_displayName}")
                    .Validate(x =>
                    {
                        if (x.Trim() == FlowNavigation.BackInputToken)
                        {
                            return ValidationResult.Success();
                        }

                        return Validate(x);
                    });

                chosenTypeName = AnsiConsole.Prompt(prompt).Trim();
                if (string.Equals(chosenTypeName, FlowNavigation.BackInputToken, StringComparison.OrdinalIgnoreCase))
                {
                    State = FlowStepState.Back;
                    return null;
                }
            }

            if (!string.IsNullOrEmpty(chosenTypeName))
            {
                allTypesDict.TryGetValue(chosenTypeName, out ModelType? chosenModelType);
                return Tuple.Create(chosenTypeName, chosenModelType);
            }

            return null;
        }

        internal string? Prompt(IFlowContext context, string title, IDictionary<string, ModelType> allTypes)
        {
            var modelDisplayNames = allTypes.Keys.ToList();
            if (modelDisplayNames.Count == 0)
            {
                return null;
            }

            var prompt = new FlowSelectionPrompt<string>()
                .Title(title)
                .AddChoices(modelDisplayNames.Order(), navigation: context.Navigation);

            var result = prompt.Show();

            State = result.State;
            return result.Value;
        }

        private ValidationResult Validate(string className)
        {
            if (Path.GetInvalidFileNameChars().Any(c => className.Contains(c, StringComparison.OrdinalIgnoreCase)))
            {
                return ValidationResult.Error($"{className} cannot contain illegal chars!");
            }

            return ValidationResult.Success();
        }

        internal FlowStepState State { get; private set; }
        private readonly string _displayName;
        private readonly ExistingClassProperties _classProperties;
    }
}
