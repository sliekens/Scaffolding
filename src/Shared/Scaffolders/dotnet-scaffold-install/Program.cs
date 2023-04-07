using System.Threading.Tasks;
using Spectre.Console.Cli;

namespace Microsoft.DotNet.Tools.Scaffold.Install
{
    internal class Program
    {
        internal static async Task Main(string[] args)
        {
            var app = new CommandApp();
            app.Configure(c =>
            {
                c.CaseSensitivity(CaseSensitivity.None);
                c.AddCommand<InstallCommand>("install");
            });

            await app.RunAsync(args);
        }
    }
}
