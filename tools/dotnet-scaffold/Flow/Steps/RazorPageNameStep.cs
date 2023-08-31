// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.DotNet.Tools.Scaffold.Commands;
using Spectre.Console;
using Spectre.Console.Flow;

namespace Microsoft.DotNet.Tools.Scaffold.Flow.Steps
{
    internal class RazorPageNameStep : IFlowStep
    {
        internal RazorPageNameStep() { }

        public string Id => nameof(ControllerNameStep);

        public string DisplayName => "Razor Page Name";

        public ValueTask ResetAsync(IFlowContext context, CancellationToken cancellationToken)
        {
            context.Unset(FlowProperties.RazorPageName);
            return new ValueTask();
        }

        public ValueTask<FlowStepResult> RunAsync(IFlowContext context, CancellationToken cancellationToken)
        {
            var prompt = new TextPrompt<string>("Enter a new [hotpink3_1]razor page[/] name (or [lightseagreen]<[/] to go back):")
            .ValidationErrorMessage("Not a valid razor page name")
            .Validate(x =>
            {
                if (x.Trim() == FlowNavigation.BackInputToken)
                {
                    return ValidationResult.Success();
                }

                return Validate(x);
            });

            var razorPageName = AnsiConsole.Prompt(prompt).Trim();
            if (string.Equals(razorPageName, FlowNavigation.BackInputToken, StringComparison.OrdinalIgnoreCase))
            {
                return new ValueTask<FlowStepResult>(FlowStepResult.Back);
            }

            SetRazorPageProperties(context, razorPageName);
            return new ValueTask<FlowStepResult>(FlowStepResult.Success);
        }

        public ValueTask<FlowStepResult> ValidateUserInputAsync(IFlowContext context, CancellationToken cancellationToken)
        {
            var command = context.GetValue<Command>(FlowProperties.ScaffolderCommand);
            if (command is null)
            {
                return new ValueTask<FlowStepResult>(FlowStepResult.Failure("Invalid command or command name!"));
            }

            var commandParseResult = context.GetValue<ParseResult>(FlowProperties.ScaffolderCommandParseResult);
            var razorPageName = commandParseResult?.GetValueForOption(DefaultCommandOptions.Name);

            if (razorPageName is null)
            {
                return new ValueTask<FlowStepResult>(FlowStepResult.Failure("No name provided"));
            }

            SetRazorPageProperties(context, razorPageName);
            return new ValueTask<FlowStepResult>(FlowStepResult.Success);
        }

        private void SetRazorPageProperties(IFlowContext context, string razorPageName)
        {
            context.Set(new FlowProperty(
                FlowProperties.RazorPageName,
                razorPageName,
                "Razor Page Name",
                isVisible: true));
        }

        private ValidationResult Validate(string name)
        {
            if (Path.GetInvalidFileNameChars().Any(c => name.Contains(c, StringComparison.OrdinalIgnoreCase)))
            {
                return ValidationResult.Error("'--name' cannot contain illegal chars");
            }

            //check razor page name is invalid
            return ValidationResult.Success();
        }
    }
}
