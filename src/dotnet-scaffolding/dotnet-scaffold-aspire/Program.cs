using Microsoft.DotNet.Scaffolding.Core.Builder;
using Microsoft.DotNet.Scaffolding.Core.CommandLine;
using Microsoft.DotNet.Scaffolding.Core.Hosting;
using Microsoft.DotNet.Scaffolding.Helpers.Services;
using Microsoft.DotNet.Scaffolding.Helpers.Services.Environment;
using Microsoft.DotNet.Tools.Scaffold.Aspire;
using Microsoft.DotNet.Tools.Scaffold.Aspire.Commands;
using Microsoft.DotNet.Tools.Scaffold.Aspire.ScaffoldSteps;
using Microsoft.Extensions.DependencyInjection;


var builder = Host.CreateScaffoldBuilder();

ConfigureServices(builder.Services);

CreateOptions(out var cachingTypeOption, out var databaseTypeOption, out var storageTypeOption,
              out var appHostProjectOption, out var projectOption, out var prereleaseOption);


var caching = builder.AddScaffolder("caching", category: "Aspire");
caching.WithOption(cachingTypeOption)
       .WithOption(appHostProjectOption)
       .WithOption(projectOption)
       .WithOption(prereleaseOption)
       .WithStep<PlaceholderCachingStep>(configurator =>
       {
           var step = configurator.Step;
           var context = configurator.Context;

           step.Type = context.GetOptionResult(cachingTypeOption);
           step.AppHostProject = context.GetOptionResult(appHostProjectOption);
           step.Project = context.GetOptionResult(projectOption);
           step.Prerelease = context.GetOptionResult(prereleaseOption);
       });

var database = builder.AddScaffolder("database", category: "Aspire");
database.WithOption(databaseTypeOption)
        .WithOption(appHostProjectOption)
        .WithOption(projectOption)
        .WithOption(prereleaseOption)
        .WithStep<PlaceholderDatabaseStep>(configurator =>
        {
            var step = configurator.Step;
            var context = configurator.Context;

            step.Type = context.GetOptionResult(databaseTypeOption);
            step.AppHostProject = context.GetOptionResult(appHostProjectOption);
            step.Project = context.GetOptionResult(projectOption);
            step.Prerelease = context.GetOptionResult(prereleaseOption);
        });

var storage = builder.AddScaffolder("storage", category: "Aspire");
storage.WithOption(storageTypeOption)
       .WithOption(appHostProjectOption)
       .WithOption(projectOption)
       .WithOption(prereleaseOption)
       .WithStep<PlaceholderStorageStep>(configurator =>
       {
           var step = configurator.Step;
           var context = configurator.Context;

           step.Type = context.GetOptionResult(storageTypeOption);
           step.AppHostProject = context.GetOptionResult(appHostProjectOption);
           step.Project = context.GetOptionResult(projectOption);
           step.Prerelease = context.GetOptionResult(prereleaseOption);
       });

var runner = builder.Build();

runner.RunAsync(args).Wait();

static void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton<PlaceholderCachingStep>();
    services.AddSingleton<PlaceholderDatabaseStep>();
    services.AddSingleton<PlaceholderStorageStep>();
    services.AddSingleton<IFileSystem, FileSystem>();
    services.AddSingleton<IEnvironmentService, EnvironmentService>();
    services.AddSingleton<IDotNetToolService, DotNetToolService>();
    services.AddSingleton<ILogger, AnsiConsoleLogger>(); // Temporary???
}

static void CreateOptions(out Option<string> cachingTypeOption, out Option<string> databaseTypeOption, out Option<string> storageTypeOption,
                          out Option<string> appHostProjectOption, out Option<string> projectOption, out Option<bool> prereleaseOption)
{
    cachingTypeOption = new Option<string>(new()
    {
        DisplayName = "Caching type",
        CliOption = "--type",
        Description = "Types of caching",
        Required = true,
        PickerType = InteractivePickerType.CustomPicker,
        CustomPickerValues = GetCmdsHelper.CachingTypeCustomValues
    });

    databaseTypeOption = new Option<string>(new()
    {
        DisplayName = "Database type",
        CliOption = "--type",
        Description = "Types of database",
        Required = true,
        PickerType = InteractivePickerType.CustomPicker,
        CustomPickerValues = GetCmdsHelper.DatabaseTypeCustomValues
    });

    storageTypeOption = new Option<string>(new()
    {
        DisplayName = "Storage type",
        CliOption = "--type",
        Description = "Types of storage",
        Required = true,
        PickerType = InteractivePickerType.CustomPicker,
        CustomPickerValues = GetCmdsHelper.StorageTypeCustomValues
    });

    appHostProjectOption = new Option<string>(new()
    {
        DisplayName = "Aspire App host project file",
        CliOption = "--apphost-project",
        Description = "Aspire App host project for the scaffolding",
        Required = true,
        PickerType = InteractivePickerType.ProjectPicker
    });

    projectOption = new Option<string>(new()
    {
        DisplayName = "Web or worker project file",
        CliOption = "--project",
        Description = "Web or worker project associated with the Aspire App host",
        Required = true,
        PickerType = InteractivePickerType.ProjectPicker
    });

    prereleaseOption = new Option<bool>(new()
    {
        DisplayName = "Include Prerelease packages?",
        CliOption = "--prerelease",
        Description = "Include prerelease package versions when installing latest Aspire components",
        Required = false,
        PickerType = InteractivePickerType.YesNo
    });
}
