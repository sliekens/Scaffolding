// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.DotNet.Scaffolding.Shared.Helpers
{
    internal static class FileSystemHelper
    {
        internal static List<string> GetFilePaths(string directoryPath, string pattern)
        {
            return Directory.EnumerateFiles(directoryPath, pattern, SearchOption.AllDirectories).ToList();
        }

        internal static string GetFirstFilePath(string directoryPath, string pattern)
        {
            return GetFilePaths(directoryPath, pattern).FirstOrDefault();
        }
    }
}
