using Microsoft.DotNet.Scaffolding.Shared.Spectre.Services;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace Microsoft.DotNet.Tools.Scaffold.Commands
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var registrations = new ServiceCollection();
            //registrations.AddSingleton<IToolService>(toolsService);

            var registrar = new TypeRegistrar(registrations);
            var app = new CommandApp(registrar);

            app.Configure(config =>
            {
                config.AddCommand<AreaCommand>("area");
                config.AddCommand<ControllerCommand>("controller");
                config.AddCommand<IdentityCommand>("identity");
                config.AddCommand<MinimalApiCommand>("minimalapi");
                config.AddCommand<RazorPageCommand>("razorpage");
                config.AddCommand<ViewCommand>("view");
            });

        }
    }
}
