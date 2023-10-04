// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using Microsoft.DotNet.Tools.Scaffold.Helpers;
using Spectre.Console.Flow;

namespace Microsoft.DotNet.Tools.Scaffold.Flow.Discoveries
{
    internal class ApiDiscovery
    {
        internal FlowStepState State { get; private set; }

        internal Tuple<Command, string>? Prompt(IFlowContext context, string title)
        {
            IDictionary<string, Tuple<Command, string>> apiOptions = ApiScaffolders;

            var prompt = new FlowSelectionPrompt<string>()
                .Title(title)
                .AddChoices(apiOptions.Keys.ToList(), navigation: context.Navigation);

            var result = prompt.Show();
            State = result.State;
            if (!string.IsNullOrEmpty(result.Value))
            {
                return ApiScaffolders.GetValueOrDefault(result.Value);
            }

            return null;
        }

        internal Tuple<Command, string>? Discover(IFlowContext context)
        {
            return Prompt(context, "Which API scaffolder do you want to use?");
        }

        internal Dictionary<string, Tuple<Command,string>>? _apiScaffolders;
        internal Dictionary<string, Tuple<Command, string>> ApiScaffolders => _apiScaffolders ??=
            new Dictionary<string, Tuple<Command, string>>()
            {
                { Helpers.Templates.EmptyApiController, Tuple.Create(DefaultCommands.ApiControllerCommand, "API Controller - Empty") },
                { Helpers.Templates.ActionsApiController, Tuple.Create(DefaultCommands.ApiControllerCommand, "API Cotroller with read/write actions") },
                { Helpers.Templates.ApiEndpoints, Tuple.Create(DefaultCommands.ApiEndpointsCommand, "API with read/write endpoints") },
                { Helpers.Templates.ApiEndpointsWithEf, Tuple.Create(DefaultCommands.ApiEndpointsCommand, "API with read/write endpoints, using EF") }
            };

        internal Dictionary<string, Tuple<Command, string>>? _apiControllerScaffolders;
        internal Dictionary<string, Tuple<Command, string>> ApiControllerScaffolders => _apiControllerScaffolders ??=
            new Dictionary<string, Tuple<Command, string>>()
            {
                { Helpers.Templates.EmptyApiController, Tuple.Create(DefaultCommands.ApiControllerCommand, "API Controller - Empty") },
                { Helpers.Templates.ActionsApiController, Tuple.Create(DefaultCommands.ApiControllerCommand, "API Cotroller with read/write actions") },
            };

        internal Dictionary<string, Tuple<Command, string>>? _apiEndpointsScaffolders;
        internal Dictionary<string, Tuple<Command, string>> ApiEndpointsScaffolders => _apiEndpointsScaffolders ??=
            new Dictionary<string, Tuple<Command, string>>()
            {
                { Helpers.Templates.ApiEndpoints, Tuple.Create(DefaultCommands.ApiEndpointsCommand, "API with read/write endpoints") },
                { Helpers.Templates.ApiEndpointsWithEf, Tuple.Create(DefaultCommands.ApiEndpointsCommand, "API with read/write endpoints, using EF") }
            };
    }
}
