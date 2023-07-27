// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.DotNet.Tools.Scaffold.Flow;
using Spectre.Console.Flow;

namespace Microsoft.DotNet.Tools.Scaffold.Flow
{
    internal static class FlowExtensions
    {
        internal static FlowNavigation Navigation(this FlowContext context)
        {
            return (FlowNavigation)context.Navigation;
        }
    }
}
