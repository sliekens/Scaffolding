// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.DotNet.Tools.Scaffold.Commands;
using Spectre.Console.Flow;

namespace Microsoft.DotNet.Tools.Scaffold.Flow.Discoveries
{
    internal class MvcDiscovery
    {
        internal MvcDiscovery() { }

        internal FlowStepState State { get; private set; }

        internal string? Prompt(IFlowContext context, string title)
        {
            var prompt = new FlowSelectionPrompt<string>()
                .Title(title)
                .AddChoices(MvcScaffolders.Keys.ToList(), navigation: context.Navigation);

            var result = prompt.Show();
            State = result.State;
            if (!string.IsNullOrEmpty(result.Value))
            {
                return MvcScaffolders.GetValueOrDefault(result.Value);
            }

            return null;
        }

        internal string? Discover(IFlowContext context)
        {
            return Prompt(context, "Which MVC scaffolder do you want to use?");
        }

        internal Dictionary<string, string>? _mvcScaffolders;
        internal Dictionary<string, string> MvcScaffolders => _mvcScaffolders ??=
            new Dictionary<string, string>()
            {
                { "MVC Controller - Empty", "MVC Controller - Empty" },
                { "MVC Cotroller with read/write actions", "MVC Cotroller with read/write actions" },
            };
    }
}

