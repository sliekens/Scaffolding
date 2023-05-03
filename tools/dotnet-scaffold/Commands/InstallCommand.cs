using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Microsoft.DotNet.Tools.Scaffold.Install
{
    internal class InstallCommand : Command<InstallCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            [CommandOption("--add-source")]
            public string Source { get; set; } = default!;

            [CommandOption("--interactive")]
            public bool? Interactive { get; set; }

            [CommandOption("--force")]
            public bool Force { get; set; } = default!;
        }

        public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
        {
            System.Diagnostics.Debugger.Launch();
            if (settings.Interactive == null || settings.Interactive is true)
            {
                ValidateArgs(settings);
            }

            bool isFile = File.Exists(settings.Source);
            bool isFolder = Directory.Exists(settings.Source);
            if (isFolder)
            {
                //get folder name
                //find config, get name
                //copy its content to content/commands/{name} in dotnet-scaffold\somewhere?
            }
            else if (isFile)
            {
                //check if nupkg file,
                if (Path.GetExtension(settings.Source).ToLower() != ".nupkg")
                {
                    AnsiConsole.WriteLine("File is not a .nupkg package, exiting");
                    return -1;
                }

                using ZipArchive archive = ZipFile.OpenRead(settings.Source);
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string fullPath = Path.Combine(BasePath, entry.FullName);

                    if (entry.FullName.EndsWith("/") || entry.FullName.EndsWith("\\"))
                    {
                        Directory.CreateDirectory(fullPath);
                    }
                    else
                    {
                        entry.ExtractToFile(fullPath, true);
                    }
                }
            }

            return 0;
        }

        private void ValidateArgs(Settings settings)
        {
            if (string.IsNullOrEmpty(settings.Source))
            {
                var sourceType = AnsiConsole.Prompt(
                   new SelectionPrompt<string>()
                       .Title("Pick a valid [underline bold]type of source[/]:")
                       .PageSize(5)
                       .AddChoices(_installSources));

                var sourcePrompt = $"Provide a valid {sourceType}:";
                string sourcePromptValue = AnsiConsole.Ask<string>(sourcePrompt);

                if (!string.IsNullOrEmpty(sourcePromptValue))
                {
                    settings.Source = sourcePromptValue;
                    //no urls supported yet
                    //https://www.nuget.org/packages/Microsoft.dotnet-scaffold/7.0.0-rc.2.22510.1
                    
                }
                else
                {
                    AnsiConsole.WriteLine("whatever dude");
                }
            }
        }

        private readonly List<string> _installSources = new List<string>() { FilePathOption, NugetSourceOption };
        private static readonly string FilePathOption = "File Path (on disk)";
        private static readonly string NugetSourceOption = "NuGet Source Url";
        private readonly string BasePath = Path.Combine("content", "commands");
    }
}

