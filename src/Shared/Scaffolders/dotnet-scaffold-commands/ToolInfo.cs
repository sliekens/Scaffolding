// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.DotNet.Tools.Scaffold.Commands
{
    public class ToolInfo
    {
        public string? Author { get; set; }
        public string? ToolDescription { get; set; }
        public string ToolName { get; set; } = default!;
        public string ToolExeName { get; set; } = default!;
        public string? ToolVersion { get; set; }
    }
}
