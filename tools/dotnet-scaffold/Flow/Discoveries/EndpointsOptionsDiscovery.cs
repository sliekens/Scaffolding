
using System.CommandLine.Parsing;
using System.Linq;
using Microsoft.DotNet.Scaffolding.Shared.Extensions;
using Microsoft.DotNet.Tools.Scaffold.Flow.Steps;
using Spectre.Console;
using Spectre.Console.Flow;

namespace Microsoft.DotNet.Tools.Scaffold.Flow.Discoveries
{
    internal class EndpointsOptionsDiscovery
    {
        internal FlowStepState State { get; private set; }

        public EndpointsOptionsDiscovery()
        {
        }

        internal EndpointsScaffolderOptions Discover(IFlowContext context)
        {
            var parseResult = context.GetValue<ParseResult>(FlowProperties.ScaffolderCommandParseResult);
            var allTokens = parseResult?.Tokens.Select(x => x.Value);
            var openApi = parseResult?.Tokens.Any(x => x.Value.Equals("--openapi"));
            var typedResults = parseResult?.Tokens.Any(x => x.Value.Equals("--typed-results"));
            var endpointsScaffolderOptions = new EndpointsScaffolderOptions
            {
                OpenApi = openApi ?? true,
                TypedResults = typedResults ?? true
            };

            if (openApi is null || typedResults is null)
            {
                endpointsScaffolderOptions = Prompt(context, endpointsScaffolderOptions) ?? endpointsScaffolderOptions;
            }

            return endpointsScaffolderOptions;
        }

        private EndpointsScaffolderOptions? Prompt(IFlowContext context, EndpointsScaffolderOptions endpointsScaffolderOptions)
        {
            var endpointsScaffolderOptionsType = typeof(EndpointsScaffolderOptions);
            var prompt = new FlowMultiSelectionPrompt<string>()
                .Title("Pick endpoint options:")
                .AddChoices(endpointsScaffolderOptionsType.GetPropertyNames(), navigation: context.Navigation);

            var result = prompt.Show();
            State = result.State;
            if (result.Value != null )
            {
                return endpointsScaffolderOptions;
            }

            return null;
        }
    }
}
