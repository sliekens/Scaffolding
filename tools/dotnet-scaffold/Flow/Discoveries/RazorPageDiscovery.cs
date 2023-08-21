// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using Microsoft.DotNet.Tools.Scaffold.Commands;
using Spectre.Console.Flow;

namespace Microsoft.DotNet.Tools.Scaffold.Flow.Discoveries
{
    internal class RazorPageDiscovery
    {
        internal RazorPageDiscovery() {}

        internal FlowStepState State { get; private set; }

        internal Tuple<Command, string>? Prompt(IFlowContext context, string title)
        {
            var prompt = new FlowSelectionPrompt<string>()
                .Title(title)
                .AddChoices(RazorPageScaffolders.Keys.ToList(), navigation: context.Navigation);

            var result = prompt.Show();
            if (!string.IsNullOrEmpty(result.Value))
            {
                State = result.State;
                return RazorPageScaffolders.GetValueOrDefault(result.Value);
            }

            return null;
        }

        internal Tuple<Command, string>? Discover(IFlowContext context)
        {
            return Prompt(context, "Which Razor Page scaffolder do you want to use?");
        }

        internal Dictionary<string, Tuple<Command, string>>? _razorPageScaffolders;
        internal Dictionary<string, Tuple<Command, string>> RazorPageScaffolders => _razorPageScaffolders ??=
            new Dictionary<string, Tuple<Command, string>>()
            {
                { "Razor Pages - Empty", Tuple.Create(DefaultCommands.RazorPagesCommand, "Razor Pages - Empty") }
            };
    }
}

