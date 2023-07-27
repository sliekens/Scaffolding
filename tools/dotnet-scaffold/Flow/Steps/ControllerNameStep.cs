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
    internal class ControllerNameStep : IFlowStep
    {
        internal ControllerNameStep(bool actions)
        {
            _actionsController = actions;
        }
        public string Id => nameof(ControllerNameStep);

        public string DisplayName => "Controller Name";

        public ValueTask ResetAsync(IFlowContext context, CancellationToken cancellationToken)
        {
            context.Unset(FlowProperties.ControllerName);
            return new ValueTask();
        }

        public ValueTask<FlowStepResult> RunAsync(IFlowContext context, CancellationToken cancellationToken)
        {
            var prompt = new TextPrompt<string>("Enter new controller name (or [lightseagreen]<[/] to go back).")
            .ValidationErrorMessage("Not a valid controller name")
            .Validate(x =>
            {
                if (x.Trim() == FlowNavigation.BackInputToken)
                {
                    return ValidationResult.Success();
                }

                return Validate(x);
            });

            var controllerName = AnsiConsole.Prompt(prompt).Trim();
            if (string.Equals(controllerName, FlowNavigation.BackInputToken, StringComparison.OrdinalIgnoreCase))
            {
                return new ValueTask<FlowStepResult>(FlowStepResult.Back);
            }

            SetControllerProperties(context, controllerName);
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
            var controllerName = commandParseResult?.GetValueForOption(DefaultCommandOptions.Name);

            if (controllerName is null)
            {
                return new ValueTask<FlowStepResult>(FlowStepResult.Failure("No area name provided"));
            }

            SetControllerProperties(context, controllerName);
            return new ValueTask<FlowStepResult>(FlowStepResult.Success);
        }

        private void SetControllerProperties(IFlowContext context, string controllerName)
        {
            context.Set(new FlowProperty(
                FlowProperties.ControllerName,
                controllerName,
                "Controller Name",
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

        private bool _actionsController;
    }
}
