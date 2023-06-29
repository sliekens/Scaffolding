using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.DotNet.Scaffolding.Shared.Spectre.Services;
using Microsoft.DotNet.Tools.Scaffold.Commands.Services;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Flow;

namespace Microsoft.DotNet.Tools.Scaffold.Commands
{
    public class Program
    {
        public async static Task<int> Main(string[] args)
        {
            var registrations = new ServiceCollection();
            var flowProvider = new FlowProvider();
            registrations.AddSingleton<IFlowProvider>(flowProvider);
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

            args = ValidateArgs(args);
            await app.RunAsync(args);
            return 0;
        }

        private static string[] ValidateArgs(string[] args)
        {
            List<string> argsList = args.ToList();
            List<string> commandNames = new List<string> { "area", "controller", "identity", "minimalapi", "razorpage", "view" };

            if (argsList.Count == 0)
            {
                var commandName = AnsiConsole.Prompt(
                   new SelectionPrompt<string>()
                       .Title("Pick a scaffolder: ")
                       .PageSize(15)
                       .AddChoices(commandNames));

                argsList.Add(commandName);
            }

            return argsList.ToArray();
        }
    }
}
