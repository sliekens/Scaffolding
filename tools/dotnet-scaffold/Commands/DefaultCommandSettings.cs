using Spectre.Console.Cli;

namespace Microsoft.DotNet.Tools.Scaffold.Commands
{
    public class ScaffoldSettings : CommandSettings
    {
        [CommandArgument(0, "[Command]")]
        public string CommandName { get; set; } = default!;
    }
}
