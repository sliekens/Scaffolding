// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.DotNet.Tools.Scaffold.Scaffolders;
using Spectre.Console.Flow;
using Microsoft.DotNet.Tools.Scaffold.Templating;
using System.IO;
using System.Linq;
using Spectre.Console;
using Microsoft.DotNet.Scaffolding.Shared.Project;
using Microsoft.DotNet.Scaffolding.Shared.Project.Workspaces;
using System.Threading.Tasks;
using System.CommandLine.Parsing;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.DotNet.Scaffolding.Shared.T4Templating;
using Microsoft.DotNet.Scaffolding.Shared.Cli.Utils;

namespace Microsoft.DotNet.Tools.Scaffold.Commands
{
    internal class ApiScaffolder : IInternalScaffolder
    {
        private readonly IFlowContext _flowContext;
        private readonly StringBuilder _commandlineString;

        public ApiScaffolder(IFlowContext flowContext)
        {
            _flowContext = flowContext;
            _commandlineString = new StringBuilder($"dotnet scaffold api ");
        }
  
        public async Task<int> ExecuteAsync()
        {
            await Task.Delay(1);
            var projectPath = _flowContext.GetValue<string>(Flow.FlowProperties.SourceProjectPath);
            var apiTemplate = _flowContext.GetValue<string>(Flow.FlowProperties.ApiScaffolderTemplate);
            _commandlineString.Append($"--project-path {projectPath} ");

            if (string.IsNullOrEmpty(projectPath) || string.IsNullOrEmpty(apiTemplate))
            {
                return -1;
            }

            switch(apiTemplate)
            {
                case "controller":
                    ExecuteEmptyScaffolders();
                    break;
                case "endpoints":
                    ExecuteEndpointScaffolders();
                    break;
            }

            PrintCommand();
            return 0;
        }

        internal void ExecuteEmptyScaffolders()
        {
            var projectFilePath = _flowContext.GetValue<string>(Flow.FlowProperties.SourceProjectPath);
            var projectFolderPath = Path.GetDirectoryName(projectFilePath);
            var controllerName = _flowContext.GetValue<string>(Flow.FlowProperties.ControllerName);
            var actionsController = _flowContext.GetValue<bool>(Flow.FlowProperties.ActionsController);
            //item name in `dotnet new` for api controller or mvc controller (empty with or without actions)
            var controllerTypeName = "apicontroller";
            var actionsParameter = actionsController ? "--actions" : string.Empty;
            //arguments for `dotnet new page`
            if (string.IsNullOrEmpty(controllerName) || string.IsNullOrEmpty(projectFolderPath))
            {
                //need a fugging name
                return;
            }

            var additionalArgs = new List<string>()
            {
                controllerTypeName,
                "--name",
                controllerName,
                "--output",
                projectFolderPath,
                actionsParameter,
            };

            DotnetCommands.ExecuteDotnetNew(projectFilePath, additionalArgs, new Scaffolding.Shared.ConsoleLogger());
        } 

        internal void ExecuteEndpointScaffolders()
        {
            //get workspace
            var roslynWorkspace = _flowContext.GetValue<RoslynWorkspace>(Flow.FlowProperties.SourceProjectWorkspace);
            //get model type
            var modelClassType = _flowContext.GetValue<ModelType>($"{Flow.FlowProperties.ModelClassType}-{Flow.FlowProperties.ModelClassDisplayName.Replace(" ", "")}");
            //get endpoints class name
            var endpointsClassName = _flowContext.GetValue<string>($"{Flow.FlowProperties.ExistingClassName}-{Flow.FlowProperties.EndpointsClassDisplayName.Replace(" ", "")}");
            if (roslynWorkspace is null || modelClassType is null || string.IsNullOrEmpty(endpointsClassName))
            {
                return;
            }

            _commandlineString.Append($"--model {modelClassType.Name} ");
            _commandlineString.Append($"--endpoints {endpointsClassName} ");
            //try getting endpoints class type
            var endpointsClassType = _flowContext.GetValue<ModelType>($"{Flow.FlowProperties.ExistingClassType}-{Flow.FlowProperties.EndpointsClassName.Replace(" ", "")}");
            if (endpointsClassType is null)
            {
                //create endpoints class and its type
                //var endpointsClassType
            }

            Templates.ScaffoldingT4Templates.TryGetValue("api endpoints", out var allMinimalApiTemplates);
            var minimalApiTemplatePath = allMinimalApiTemplates?.First();
            if (string.IsNullOrEmpty(minimalApiTemplatePath))
            {
                return;
            }

            // var minimalApiTemplatePath = minimalApiTemplates.First(x => x.Contains(GetTemplateName(model, existingEndpointsFile: false)));
            var minimalApiT4Generator = T4TemplateHelper.CreateT4Generator(minimalApiTemplatePath);
            TemplateInvoker templateInvoker = new();
            /*            var templateModel = new MinimalApiModel(modelTypeAndContextModel.ModelType, modelTypeAndContextModel.DbContextFullName, model.EndpintsClassName)
                        {
                            EndpointsName = model.EndpintsClassName,
                            EndpointsNamespace = namespaceName,
                            ModelMetadata = modelTypeAndContextModel.ContextProcessingResult?.ModelMetadata,
                            NullableEnabled = "enable".Equals(ProjectContext?.Nullable, StringComparison.OrdinalIgnoreCase),
                            OpenAPI = model.OpenApi,
                            MethodName = $"Map{modelTypeAndContextModel.ModelType.Name}Endpoints",
                            DatabaseProvider = model.DatabaseProvider,
                            UseTypedResults = !model.NoTypedResults
                        };
            */
            /*            var dictParams = new Dictionary<string, object>()
                        {
                            { "Model" , templateModel }
                        };

                        var result = templateInvoker.InvokeTemplate(minimalApiT4Generator, dictParams);
                        var endpointsFilePath = endpointsModel?.TypeSymbol?.Locations.FirstOrDefault()?.SourceTree?.FilePath ?? ValidateAndGetOutputPath(model);

                        //endpoints file exists, use CodeAnalysis to add required clauses.
                        if (FileSystem.FileExists(endpointsFilePath))
                        {
                            //get method block with the api endpoints.
                            string membersBlockText = await CodeGeneratorActionsService.ExecuteTemplate(GetTemplateName(model, existingEndpointsFile: true), TemplateFolders, templateModel);
                            var className = model.EndpintsClassName;
                            await AddEndpointsMethod(membersBlockText, endpointsFilePath, className, templateModel);
                        }*/

            /*            using (var sourceStream = new MemoryStream(Encoding.UTF8.GetBytes(result)))
                        {
                            await CodeGeneratorHelper.AddFileHelper(FileSystem, endpointsFilePath, sourceStream);
                        }*/
        }

        internal void PrintCommand()
        {
            var parseResult = _flowContext.GetValue<ParseResult>(Flow.FlowProperties.ScaffolderCommandParseResult);
            var commandString = _flowContext.GetValue<string>(Flow.FlowProperties.ScaffolderCommandString);
            var nonInteractive = parseResult?.Tokens.Any(x => x.Value.Equals("--non-interactive"));
            if (!nonInteractive.GetValueOrDefault())
            {
                AnsiConsole.WriteLine("To execute the command non-interactively, use:");
                AnsiConsole.Write(new Markup($"'[springgreen1]{_commandlineString.ToString().Trim()}[/]'\n"));
            }
        }
    }
}

