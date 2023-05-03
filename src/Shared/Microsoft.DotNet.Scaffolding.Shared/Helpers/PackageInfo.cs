namespace Microsoft.DotNet.Scaffolding.Shared.Helpers
{
    public class PackageInfo
    {
        public string PackageName { get; set; }
        public string PackageVersion { get; set; }
        public string Author { get; set; }
        public ToolInfo ToolInfo { get; set; }
        public string PackageUri { get; set; }
        public string PackageOutputFolderName { get; set; }
    }

    public class ToolInfo
    {
        public string ToolName { get; set; }
        public string ToolDescription { get; set; }
        public string ToolExeName { get; set; }
    }
}
