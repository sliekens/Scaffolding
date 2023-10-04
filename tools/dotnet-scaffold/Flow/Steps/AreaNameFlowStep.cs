// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.DotNet.Tools.Scaffold.Helpers;
using Spectre.Console;
using Spectre.Console.Flow;

namespace Microsoft.DotNet.Tools.Scaffold.Flow.Steps
{
    internal class AreaNameFlowStep : IFlowStep
    {
        public string Id => nameof(AreaNameFlowStep);

        public string DisplayName => "Area Name";

        public ValueTask ResetAsync(IFlowContext context, CancellationToken cancellationToken)
        {
            context.Unset(FlowProperties.AreaName);
            return new ValueTask();
        }

        public ValueTask<FlowStepResult> RunAsync(IFlowContext context, CancellationToken cancellationToken)
        {
            var prompt = new TextPrompt<string>("Enter a new [hotpink3_1]area name[/] (or [lightseagreen]<[/] to go back):")
            .ValidationErrorMessage("Not a valid area name")
            .Validate(x =>
            {
                if (x.Trim() == FlowNavigation.BackInputToken)
                {
                    return ValidationResult.Success();
                }

                return Validate(x);
            });

            var areaName = AnsiConsole.Prompt(prompt).Trim();
            if (string.Equals(areaName, FlowNavigation.BackInputToken, StringComparison.OrdinalIgnoreCase))
            {
                return new ValueTask<FlowStepResult>(FlowStepResult.Back);
            }

            SetAreaProperties(context, areaName);
            return new ValueTask<FlowStepResult>(FlowStepResult.Success);
        }

        public ValueTask<FlowStepResult> ValidateUserInputAsync(IFlowContext context, CancellationToken cancellationToken)
        {
            var command = context.GetValue<Command>(FlowProperties.ScaffolderCommand);
            if (command is null || !command.Name.Equals("area"))
            {
                return new ValueTask<FlowStepResult>(FlowStepResult.Failure("Invalid command or command name!"));
            }

            var commandParseResult = context.GetValue<ParseResult>(FlowProperties.ScaffolderCommandParseResult);
            var areaName = commandParseResult?.GetValueForOption(DefaultCommandOptions.Name);

            if (areaName is null)
            {
                return new ValueTask<FlowStepResult>(FlowStepResult.Failure("No area name provided"));
            }

            SetAreaProperties(context, areaName);
            return new ValueTask<FlowStepResult>(FlowStepResult.Success);
        }

        private void SetAreaProperties(IFlowContext context, string areaName)
        {
            context.Set(new FlowProperty(
                FlowProperties.AreaName,
                areaName,
                "Area Name",
                isVisible: true));
        }

        private ValidationResult Validate(string areaName)
        {
            if (Path.GetInvalidFileNameChars().Any(c => areaName.Contains(c, StringComparison.OrdinalIgnoreCase)))
            {
                return ValidationResult.Error("area cannot contain illegal chars");
            }

            //check area name is invalid

            return ValidationResult.Success();
        }
    }
}
