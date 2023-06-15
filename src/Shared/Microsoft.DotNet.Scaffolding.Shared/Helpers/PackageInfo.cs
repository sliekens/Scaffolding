// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO;

namespace Microsoft.DotNet.Scaffolding.Shared.Helpers
{
    public class ToolInfo
    {
        public string Author { get; set; }
        public string ToolDescription { get; set; }
        public string ToolName { get; set; }
        public string ToolExeName { get; set; }
        public string ToolVersion { get; set; }
/*        public string GetExePath(string basePath)
        {
            return Path.Combine(basePath, ToolName, ToolVersion, ToolExeName);
        }*/
    }
}
