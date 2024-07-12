// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.DotNet.Scaffolding.Core.CommandLine;

namespace Microsoft.DotNet.Scaffolding.Core.Builder;

[method: SetsRequiredMembers]
public abstract class Option(OptionConfig config)
{
    public required string DisplayName { get; init; } = config.DisplayName;
    public string? CliOption { get; init; } = config.CliOption;
    public bool Required { get; init; } = config.Required;
    public string? Description { get; init; } = config.Description;
    public required InteractivePickerType PickerType { get; init; } = config.PickerType;
    public required IEnumerable<string>? CustomPickerValues { get; init; } = config.CustomPickerValues;

    internal abstract System.CommandLine.Option ToCliOption();
    internal abstract Parameter ToParameter();
}
