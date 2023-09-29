// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.CommandLine.Parsing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Spectre.Console.Flow;

namespace Microsoft.DotNet.Tools.Scaffold.Flow.Steps
{
    internal class EndpointsScaffolderOptionsFlowStep : IFlowStep
    {
        public string Id => nameof(EndpointsScaffolderOptionsFlowStep);

        public string DisplayName => "Endpoints Options";

        public ValueTask ResetAsync(IFlowContext context, CancellationToken cancellationToken)
        {
            context.Unset(FlowProperties.EndpointsScaffolderOptions);
        }

        public ValueTask<FlowStepResult> RunAsync(IFlowContext context, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public ValueTask<FlowStepResult> ValidateUserInputAsync(IFlowContext context, CancellationToken cancellationToken)
        {
            var endpointsScaffolderOptions = context.GetValue<EndpointsScaffolderOptions>(FlowProperties.EndpointsScaffolderOptions);
            var parseResult = context.GetValue<ParseResult>(FlowProperties.ScaffolderCommandParseResult);
            var nonInteractive = parseResult?.Tokens.Any(x => x.Value.Equals("--non-interactive"));
            var openApi = parseResult?.Tokens.Any(x => x.Value.Equals("--openapi"));
            var typedResults = parseResult?.Tokens.Any(x => x.Value.Equals("--typed-results"));

            if (endpointsScaffolderOptions is null && nonInteractive.GetValueOrDefault())
            { 
                endpointsScaffolderOptions = new EndpointsScaffolderOptions()
                {
                    TypedResults = typedResults ?? true,
                    OpenApi = openApi ?? true
                };

                SetEndpointsScaffolderOptions(context, endpointsScaffolderOptions);
                return new ValueTask<FlowStepResult>(FlowStepResult.Success);
            }
            else if ()
            {
                SetEndpointsScaffolderOptions(context, endpointsScaffolderOptions);
            }


            return new ValueTask<FlowStepResult>(FlowStepResult.Success);
        }

        private void SetEndpointsScaffolderOptions(IFlowContext context, EndpointsScaffolderOptions endpointsOptions)
        {
            context.Set(new FlowProperty(
                FlowProperties.EndpointsScaffolderOptions,
                endpointsOptions,
                DisplayName,
                isVisible: true));
        }
    }

    internal class EndpointsScaffolderOptions
    {
        public bool TypedResults { get; set; }
        public bool OpenApi { get; set; }
    }
}
