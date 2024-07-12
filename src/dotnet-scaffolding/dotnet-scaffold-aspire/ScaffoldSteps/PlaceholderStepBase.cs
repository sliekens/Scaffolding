// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.DotNet.Scaffolding.Core.Scaffolders;
using System.Threading;
using Microsoft.DotNet.Scaffolding.Core.Steps;
using System.Threading.Tasks;
using Microsoft.DotNet.Tools.Scaffold.Aspire.Commands;

namespace Microsoft.DotNet.Tools.Scaffold.Aspire.ScaffoldSteps;

internal abstract class PlaceholderStepBase<T>(T command) : ScaffoldStep where T : ICommandWithSettings
{
    protected readonly T _command = command;

    public string? Type { get; set; }
    public string? AppHostProject { get; set; }
    public string? Project { get; set; }
    public bool Prerelease { get; set; }

    public override async Task ExecuteAsync(ScaffolderContext context, CancellationToken cancellationToken = default)
    {
        await _command.ExecuteAsync(new CommandSettings()
        {
            Type = Type,
            AppHostProject = AppHostProject,
            Project = Project,
            Prerelease = Prerelease
        });
    }
}
