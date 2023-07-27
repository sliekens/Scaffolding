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

namespace Microsoft.DotNet.Tools.Scaffold.Commands
{
    internal class ApiScaffolder : IInternalScaffolder
    {
        private readonly IFlowContext _flowContext;

        public ApiScaffolder(IFlowContext flowContext)
        {
            _flowContext = flowContext;
        }
  
        public async Task<int> ExecuteAsync()
        {
            Debugger.Launch();
            await Task.Delay(1);
            AnsiConsole.WriteLine("Executing Minimal API Scaffolder!\n");
            StringBuilder fullCommandStringBuilder = new($"dotnet scaffold minimalapi ");
            //get workspace
            var roslynWorkspace = _flowContext.GetValue<RoslynWorkspace>(Flow.FlowProperties.SourceProjectWorkspace);
            //get model type
            var modelClassType = _flowContext.GetValue<ModelType>($"{Flow.FlowProperties.ModelClassType}-{Flow.FlowProperties.ModelClassDisplayName.Replace(" ", "")}");
            //get endpoints class name
            var endpointsClassName = _flowContext.GetValue<string>($"{Flow.FlowProperties.ExistingClassName}-{Flow.FlowProperties.EndpointsClassDisplayName.Replace(" ", "")}");
            if (roslynWorkspace is null || modelClassType is null || string.IsNullOrEmpty(endpointsClassName))
            {
                return -1;
            }

            var projectPath = _flowContext.GetValue<string>(Flow.FlowProperties.SourceProjectPath);
            fullCommandStringBuilder.Append($"--project-path {projectPath} ");
            fullCommandStringBuilder.Append($"--model {modelClassType.Name} ");
            fullCommandStringBuilder.Append($"--endpoints {endpointsClassName} ");
            fullCommandStringBuilder.Append($"--non-interactive");
            //try getting endpoints class type
            var endpointsClassType = _flowContext.GetValue<ModelType>($"{Flow.FlowProperties.ExistingClassType}-{Flow.FlowProperties.EndpointsClassName.Replace(" ", "")}");
            if (endpointsClassType is null)
            {
                //create endpoints class and its type
                //var endpointsClassType
            }

            var allMinimalApiTemplates = GetT4Templates();
            var minimalApiTemplatePath = allMinimalApiTemplates.First();
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

            PrintCommand(fullCommandStringBuilder.ToString());
            return 0;
        }

        public void PrintCommand(string fullCommand)
        {
            //var fullCommand = _flowContext.GetValue<string>(Flow.FlowProperties.ScaffolderCommandString);
            var parseResult = _flowContext.GetValue<ParseResult>(Flow.FlowProperties.ScaffolderCommandParseResult);
            var commandString = _flowContext.GetValue<string>(Flow.FlowProperties.ScaffolderCommandString);
            var nonInteractive = parseResult?.Tokens.Any(x => x.Value.Equals("--non-interactive"));
            if (!nonInteractive.GetValueOrDefault())
            {
                AnsiConsole.WriteLine("To execute the command non-interactively, use:");
                AnsiConsole.WriteLine($"`{fullCommand}`");
            }
        }

        private List<string> GetT4Templates()
        {
            var baseToolFolder = ScaffolderCommandsHelper.ScaffoldToolFolder;
            var minimalApiTemplates = ScaffolderCommandsHelper.MinimalApiTemplates.Select(x => Path.Combine(baseToolFolder, x)).ToList();
            minimalApiTemplates.RemoveAll(path => !File.Exists(path));
            return minimalApiTemplates;
        }
    }
}

