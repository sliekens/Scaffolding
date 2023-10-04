// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.CommandLine;
using System.Linq;

namespace Microsoft.DotNet.Tools.Scaffold.Extensions
{
    internal static class SystemCommandlineExtensions
    {
        internal static T? GetValueForOptionWithName<T>(this ParseResult parseResult, Command? command, string optionName) where T : struct
        {
            T? optionValue = default;
            if (command is null || string.IsNullOrEmpty(optionName))
            {
                return optionValue;
            }
            Option<T>? optionToCheckFor = command?.Options.FirstOrDefault(x => x.Name.Equals(optionName)) as Option<T>;

            if (optionToCheckFor != null)
            {
                optionValue = parseResult?.GetValueForOption(optionToCheckFor);
            }

            return optionValue;
        }

        internal static T? GetClassValueForOptionWithName<T>(this ParseResult parseResult, Command? command, string optionName) where T : class
        {
            T? optionValue = default;
            if (command is null || string.IsNullOrEmpty(optionName))
            {
                return optionValue;
            }
            Option<T>? optionToCheckFor = command?.Options.FirstOrDefault(x => x.Name.Equals(optionName)) as Option<T>;

            if (optionToCheckFor != null)
            {
                optionValue = parseResult?.GetValueForOption(optionToCheckFor);
            }

            return optionValue;
        }

        internal static T? GetValueForArgumentWithName<T>(this ParseResult parseResult, Command? command, string argName) where T : class
        {
            T? argValue = default;
            if (command is null || string.IsNullOrEmpty(argName))
            {
                return argValue;
            }
            Argument<T>? argToCheckFor = command?.Arguments.FirstOrDefault(x => x.Name.Equals(argName)) as Argument<T>;

            if (argToCheckFor != null)
            {
                argValue = parseResult?.GetValueForArgument(argToCheckFor);
            }

            return argValue;
        }

        internal static List<Command> GetAllCommandsAndSubcommands(this List<Command> commands)
        {
            var allCommands = new List<Command>();
            foreach (var command in commands)
            {
                allCommands.Add(command);
                allCommands.AddRange(command.Subcommands.ToList().GetAllCommandsAndSubcommands());
            }
            return allCommands;
        }

        internal static bool? IsNonInteractive(this ParseResult? parseResult)
        {
            if (parseResult is not null)
            {
                return parseResult.Tokens.Any(x => x.Value.Equals("non-interactive"));
            }

            return null;
        }
    }
}
