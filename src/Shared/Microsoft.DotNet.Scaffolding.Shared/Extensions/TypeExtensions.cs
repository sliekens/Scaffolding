// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.DotNet.Scaffolding.Shared.Extensions
{
    internal static class TypeExtensions
    {
        internal static IList<string> GetPropertyNames(this Type type)
        {
            var properties = type?.GetProperties().Select(x => x.Name);
            if (properties != null && properties.Any())
            {
                return properties.ToList();
            }

            return null;
        }
    }
}
