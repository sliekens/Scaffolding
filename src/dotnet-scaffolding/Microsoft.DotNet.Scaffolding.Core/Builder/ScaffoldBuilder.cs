// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.DotNet.Scaffolding.Core.Scaffolders;
using Microsoft.DotNet.Scaffolding.Core.Steps;

namespace Microsoft.DotNet.Scaffolding.Core.Builder;

internal class ScaffoldBuilder(string name, string category) : IScaffoldBuilder
{
    private readonly List<Option> _options = [];
    private readonly List<ScaffoldStepPreparer> _steps = [];
    private readonly string _name = name.ToLowerInvariant();
    private readonly string _displayName = System.Globalization.CultureInfo.CurrentUICulture.TextInfo.ToTitleCase(name);
    private readonly string _category = category;

    internal string Name => _name;
    internal string DisplayName => _displayName;
    internal string Category => _category;
    internal IEnumerable<Option> Options => _options;
    internal IEnumerable<ScaffoldStepPreparer> Steps => _steps;

    public IScaffoldBuilder WithOption(Option option)
    {
        _options.Add(option);
        return this;
    }

    public IScaffoldBuilder WithStep<TStep>(Action<ScaffoldStepConfigurator<TStep>>? preExecute = null, Action<ScaffoldStepConfigurator<TStep>>? postExecute = null) where TStep : ScaffoldStep
    {
        var preparer = new ScaffoldStepPreparer<TStep>()
        {
            PreExecute = preExecute,
            PostExecute = postExecute
        };

        _steps.Add(preparer);
        return this;
    }

    public IScaffolder Build(IServiceProvider serviceProvider)
    {
        List<ScaffoldStep> steps = [];
        
        foreach (var step in _steps)
        {
            var stepInstance = serviceProvider.GetService(step.GetStepType())
                ?? throw new InvalidOperationException("We should log an error here and say we couldn't find the step.");

            steps.Add((ScaffoldStep)stepInstance);    
        }
        return new Scaffolder(Name, DisplayName, Category, _options, steps, _steps);
    }
}
