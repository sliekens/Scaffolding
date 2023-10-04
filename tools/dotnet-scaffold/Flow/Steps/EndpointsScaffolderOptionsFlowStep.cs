// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.DotNet.Tools.Scaffold.Commands;
using Microsoft.DotNet.Tools.Scaffold.Extensions;
using Microsoft.DotNet.Tools.Scaffold.Flow.Discoveries;
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
            return new ValueTask();
        }

        public ValueTask<FlowStepResult> RunAsync(IFlowContext context, CancellationToken cancellationToken)
        {
            EndpointsOptionsDiscovery endpointsDiscovery = new EndpointsOptionsDiscovery();
            EndpointsScaffolderOptions  endpointsScaffolderOptions = endpointsDiscovery.Discover(context);

            if (endpointsDiscovery.State.IsNavigation())
            {
                return new ValueTask<FlowStepResult>(new FlowStepResult { State = endpointsDiscovery.State });
            }

            SetEndpointsScaffolderOptions(context, endpointsScaffolderOptions);
            return new ValueTask<FlowStepResult>(FlowStepResult.Success);
        }

        public ValueTask<FlowStepResult> ValidateUserInputAsync(IFlowContext context, CancellationToken cancellationToken)
        {
            var endpointsScaffolderOptions = context.GetValue<EndpointsScaffolderOptions>(FlowProperties.EndpointsScaffolderOptions);
            var command = context.GetValue<System.CommandLine.Command>(FlowProperties.ScaffolderCommand);
            var parseResult = context.GetValue<ParseResult>(FlowProperties.ScaffolderCommandParseResult);
            var typedResults = parseResult?.GetValueForOptionWithName<bool>(command, "typed-results");
            var openApi = parseResult?.GetValueForOptionWithName<bool>(command, "openapi");
            var nonInteractive = parseResult?.IsNonInteractive();
            //var openApi = parseResult?.Tokens.Any(x => x.Value.Equals("--openapi"));

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
            else if (endpointsScaffolderOptions is null && !nonInteractive.GetValueOrDefault())
            {
                return new ValueTask<FlowStepResult>(FlowStepResult.Failure("endpoints scaffolder options missing"));
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
