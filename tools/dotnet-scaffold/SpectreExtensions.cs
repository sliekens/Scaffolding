// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Spectre.Console;

namespace Microsoft.DotNet.Tools.Scaffold
{
    public static class SpectreExtensions
    {
        public static Status WithSpinner(this Status status)
        {
            var style = Style.Parse("lightseagreen");
            return status
                .AutoRefresh(true)
                .Spinner(Spinner.Known.Aesthetic)
                .SpinnerStyle(style);
        }
    }
}
