// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.DotNet.Scaffolding.Core.CommandLine;

namespace Microsoft.DotNet.Scaffolding.Core.Builder;

[method: SetsRequiredMembers]
public class Option<T>(OptionConfig config) : Option(config)
{
    private System.CommandLine.Option<T>? _cliOption;

    internal override System.CommandLine.Option ToCliOption()
    {
        _cliOption ??= new System.CommandLine.Option<T>(FixedName);

        return _cliOption;
    }

    internal override Parameter ToParameter()
    {
        return new Parameter()
        {
            Name = FixedName,
            DisplayName = DisplayName,
            Required = Required,
            Description = Description,
            Type = Parameter.GetBaseType<T>(),
            PickerType = PickerType,
            CustomPickerValues = CustomPickerValues
        };
    }

    private string FixedName => CliOption ?? $"--{DisplayName.ToLowerInvariant().Replace(" ", "-")}";
}
