// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.DotNet.Tools.Scaffold.Aspire.Commands;

internal class CommandSettings
{
    public string? Type { get; set; }

    public string? AppHostProject { get; set; }

    public string? Project { get; set; }

    public bool Prerelease { get; set; }
}
