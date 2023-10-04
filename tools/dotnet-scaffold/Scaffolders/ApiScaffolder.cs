// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.DotNet.Scaffolding.Shared.Cli.Utils;
using Microsoft.DotNet.Scaffolding.Shared.CodeModifier;
using Microsoft.DotNet.Scaffolding.Shared.CodeModifier.CodeChange;
using Microsoft.DotNet.Scaffolding.Shared.Project;
using Microsoft.DotNet.Scaffolding.Shared.Project.Workspaces;
using Microsoft.DotNet.Scaffolding.Shared.ProjectModel;
using Microsoft.DotNet.Scaffolding.Shared.T4Templating;
using Microsoft.DotNet.Tools.Scaffold.Scaffolders;
using Microsoft.DotNet.Tools.Scaffold.Templates.Endpoints;
using Spectre.Console;
using Spectre.Console.Flow;
using Project = Microsoft.CodeAnalysis.Project;
using Document = Microsoft.CodeAnalysis.Document;
using Microsoft.Build.Evaluation;
using System.Xml.Linq;
using Microsoft.DotNet.Tools.Scaffold.Flow.Steps;
using Microsoft.DotNet.Tools.Scaffold.Helpers;

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
            Debugger.Launch();
            await Task.Delay(1);
            var projectPath = _flowContext.GetValue<string>(Flow.FlowProperties.SourceProjectPath);
            var apiTemplate = _flowContext.GetValue<string>(Flow.FlowProperties.ApiScaffolderTemplate);
            _commandlineString.Append($"--project-path {projectPath} ");

            if (string.IsNullOrEmpty(projectPath) || string.IsNullOrEmpty(apiTemplate))
            {
                return -1;
            }

            if (apiTemplate.Contains("controller", StringComparison.OrdinalIgnoreCase))
            {
                ExecuteEmptyScaffolders();
            }
            else if (apiTemplate.Contains("endpoints", StringComparison.OrdinalIgnoreCase))
            {
                await ExecuteEndpointScaffolders(apiTemplate);
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

        internal async Task ExecuteEndpointScaffolders(string endpointsTemplate)
        {
            //get workspace
            var projectContext = _flowContext.GetValue<IProjectContext>(Flow.FlowProperties.SourceProjectContext);
            //get workspace
            var roslynWorkspace = _flowContext.GetValue<RoslynWorkspace>(Flow.FlowProperties.SourceProjectWorkspace);
            var roslynProject = roslynWorkspace?.CurrentSolution.Projects.FirstOrDefault(p => p.AssemblyName.Equals(projectContext?.AssemblyName, StringComparison.OrdinalIgnoreCase));
            //get model type
            var modelClassType = _flowContext.GetValue<ModelType>($"{Flow.FlowProperties.ModelClassType}-{Flow.FlowProperties.ModelClassDisplayName.Replace(" ", "")}");
            //get endpoints class name
            var endpointsClassName = _flowContext.GetValue<string>($"{Flow.FlowProperties.ExistingClassName}-{Flow.FlowProperties.EndpointsClassDisplayName.Replace(" ", "")}");
            //get endpoints options
            var endpointsOptions = _flowContext.GetValue<EndpointsScaffolderOptions>(Flow.FlowProperties.EndpointsScaffolderOptions);
            //get dbcontext if its being used
            var dbContextClassType = _flowContext.GetValue<ModelType>($"{Flow.FlowProperties.ExistingClassType}-{Flow.FlowProperties.DbContextClassDisplayName.Replace(" ", "")}");

            if (roslynWorkspace is null ||
                roslynProject is null ||
                modelClassType is null ||
                endpointsOptions is null ||
                string.IsNullOrEmpty(endpointsClassName))
            {
                return;
            }

            var modelTypesLocator = new ModelTypesLocator(roslynWorkspace);
            _commandlineString.Append($"--model {modelClassType.Name} ");
            _commandlineString.Append($"--endpoints {endpointsClassName} ");
            //try getting endpoints class type
            var endpointsClassType = _flowContext.GetValue<ModelType>($"{Flow.FlowProperties.ExistingClassType}-{Flow.FlowProperties.EndpointsClassName.Replace(" ", "")}");
            if (endpointsClassType is null)
            {
                endpointsClassType = new ModelType()
                {
                    FullName = endpointsClassName,
                    Name = endpointsClassName,
                };
            }

            var t4TemplatePath = T4TemplateHelper.GetTemplateT4File(endpointsTemplate);
            var t4TemplatingGenerator = T4TemplateHelper.CreateT4Generator(t4TemplatePath);
            TemplateInvoker templateInvoker = new();

            var templateModel = new EndpointsModel(modelClassType, endpointsClassName)
            {
                NullableEnabled = "enable".Equals(projectContext?.Nullable, StringComparison.OrdinalIgnoreCase),
                OpenAPI = endpointsOptions.OpenApi,
                MethodName = $"Map{modelClassType.Name}Endpoints",
                UseTypedResults = endpointsOptions.TypedResults
            };

            var dictParams = new Dictionary<string, object>()
            {
                { "Model" , templateModel }
            };

            var membersBlockText = templateInvoker.InvokeTemplate(t4TemplatingGenerator, dictParams);
            var endpointsFileDocument = GetEndpointsDocument(roslynProject, endpointsClassType);
            await AddEndpointsMethod(templateModel, endpointsFileDocument, membersBlockText, roslynWorkspace);
            var programType = modelTypesLocator.GetType("<Program>$").FirstOrDefault() ?? modelTypesLocator.GetType("Program").FirstOrDefault();
            var programDocument = GetDocumentFromDisk(roslynProject, programType);
            await ModifyProgramCs(templateModel, roslynProject, endpointsFileDocument, programDocument);
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

        internal string? ValidateAndGetOutputPath(string className, string projectPath, string? relativeFolderPath = null)
        {
            string? appBasePath = File.Exists(projectPath) ? Path.GetDirectoryName(projectPath) : projectPath;
            if (string.IsNullOrEmpty(appBasePath))
            {
                return null;
            }

            string outputFileName = $"{className}.cs";
            string outputFolder = string.IsNullOrEmpty(relativeFolderPath)
                ? appBasePath
                : Path.Combine(appBasePath, relativeFolderPath);

            var outputPath = Path.Combine(outputFolder, outputFileName);
            return outputPath;
        }

        internal async Task<bool> AddEndpointsMethod(EndpointsModel templateModel, Document? document, string membersBlockText, Workspace roslynWorkspace)
        {
            if (document != null &&
                !string.IsNullOrEmpty(document.FilePath) &&
                !string.IsNullOrEmpty(membersBlockText) &&
                !string.IsNullOrEmpty(templateModel.EndpointsName) &&
                templateModel != null)
            {
                var docEditor = await DocumentEditor.CreateAsync(document);
                if (docEditor is null)
                {
                    return false;
                }

                //Get class syntax node to add members to the class
                var docRoot = docEditor.OriginalRoot as CompilationUnitSyntax;
                //create CodeFile just to add usings
                var endpointsCodeFile = new CodeFile { Usings = AddEndpointsUsings(templateModel).ToArray() };
                var docBuilder = new DocumentBuilder(docEditor, endpointsCodeFile, new MSIdentity.Shared.ConsoleLogger());
                var newRoot = docBuilder.AddUsings(new CodeChangeOptions());
                var classNode = newRoot.DescendantNodes().FirstOrDefault(node => node is ClassDeclarationSyntax classDeclarationSyntax && classDeclarationSyntax.Identifier.ValueText.Contains(templateModel.EndpointsName));
                //get namespace node just for the namespace name.
                var namespaceSyntax = classNode?.Parent?.DescendantNodes().FirstOrDefault(node => node is NamespaceDeclarationSyntax nsDeclarationSyntax || node is FileScopedNamespaceDeclarationSyntax fsDeclarationSyntax);
                templateModel.EndpointsNamespace = string.IsNullOrEmpty(namespaceSyntax?.ToString()) ? templateModel.EndpointsNamespace : namespaceSyntax?.ToString();

                //if a normal ClassDeclarationSyntax, add static method to this class
                if (classNode is not null &&
                    classNode.Parent is not null &&
                    classNode is ClassDeclarationSyntax classDeclaration)
                {
                    SyntaxNode? classParentSyntax = null;
                    //if class is not static, create a new class in the same file
                    if (!classDeclaration.Modifiers.Any(x => x.Text.Equals(SyntaxFactory.Token(SyntaxKind.StaticKeyword).Text)))
                    {
                        classParentSyntax = classDeclaration.Parent;
                        classDeclaration = SyntaxFactory.ClassDeclaration($"{templateModel.ModelType.Name}Endpoints")
                            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword)))
                            .NormalizeWhitespace()
                            .WithLeadingTrivia(SyntaxFactory.CarriageReturnLineFeed, SyntaxFactory.CarriageReturnLineFeed);
                    }

                    var modifiedClass = classDeclaration.AddMembers(
                        SyntaxFactory.GlobalStatement(SyntaxFactory.ParseStatement(membersBlockText)).WithLeadingTrivia(SyntaxFactory.Tab));

                    //modify class parent by adding class, classParentSyntax should be null if given class is static.
                    if (classParentSyntax != null)
                    {
                        classParentSyntax = classParentSyntax.InsertNodesAfter(classNode.Parent.ChildNodes().Last(), new List<SyntaxNode>() { modifiedClass });
                        newRoot = newRoot.ReplaceNode(classNode.Parent, classParentSyntax);
                    }
                    //modify given class
                    else
                    {
                        newRoot = newRoot.ReplaceNode(classNode, modifiedClass);
                    }
                }
                //check if its a minimal class with no class declarations (using top level statements)
                else
                {
                    //create a ClassDeclarationSyntax, add the static endpoints method to the class
                    var newClassDeclaration = SyntaxFactory.ClassDeclaration($"{templateModel.ModelType.Name}Endpoints")
                        .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword)))
                        .NormalizeWhitespace()
                        .WithLeadingTrivia(SyntaxFactory.CarriageReturnLineFeed, SyntaxFactory.CarriageReturnLineFeed);
                    newClassDeclaration = newClassDeclaration.AddMembers(
                        SyntaxFactory.GlobalStatement(SyntaxFactory.ParseStatement(membersBlockText)).WithLeadingTrivia(SyntaxFactory.Tab));
                    //add members at the end of the namespace node.
                    //replace namespace node in newRoot
                    newRoot = newRoot.InsertNodesAfter(newRoot.ChildNodes().Last(), new List<SyntaxNode> { newClassDeclaration });
                }

                if (docRoot != null && docEditor != null)
                {
                    newRoot = CodeAnalysis.Formatting.Formatter.Format(newRoot, roslynWorkspace) as CompilationUnitSyntax;
                    if (newRoot is null)
                    {
                        return false;
                    }

                    docEditor.ReplaceNode(docRoot, newRoot);
                    var classFileSourceTxt = await docEditor.GetChangedDocument().GetTextAsync();
                    var classFileTxt = classFileSourceTxt?.ToString();
                    if (!string.IsNullOrEmpty(classFileTxt))
                    {
                        //write to endpoints class path.
                        File.WriteAllText(document.FilePath, classFileTxt);
                        return true;
                    }
                }
            }

            return false;
        }

        private async Task ModifyProgramCs(
            EndpointsModel templateModel,
            Project project,
            Document? endpointsDocument,
            Document? programDocument)
        {
            string? mapMethodName = templateModel.MethodName;
            CodeModifierConfig? minimalApiChangesConfig = GetMinimalApiCodeModifierConfig();
            var programCsFile = minimalApiChangesConfig?.Files.FirstOrDefault();
            if (programCsFile is null ||
                programCsFile.Methods is null ||
                !programCsFile.Methods.Any() ||
                programDocument is null ||
                string.IsNullOrEmpty(programDocument.FilePath) ||
                endpointsDocument is null)
            {
                return;
            }

            string? endpointsNamespace = await GetNamespaceFromDocumentAsync(endpointsDocument, templateModel.EndpointsName);
            programCsFile.Usings = new string[] { endpointsNamespace ?? string.Empty };
            //Modifying Program.cs document
            var docEditor = await DocumentEditor.CreateAsync(programDocument);
            var docRoot = docEditor.OriginalRoot as CompilationUnitSyntax;
            var docBuilder = new DocumentBuilder(docEditor, programCsFile, new MSIdentity.Shared.ConsoleLogger());
            //adding usings
            var newRoot = docBuilder.AddUsings(new CodeChangeOptions());
            var useTopLevelsStatements = await ProjectModifierHelper.IsUsingTopLevelStatements(project.Documents.ToList());
            //add code snippets/changes.
            //should only include one change to add $"app.Map{MODEL}Method" to the Program.cs file. Check the minimalApiChanges.json for more info.
            var addMethodMapping = programCsFile.Methods.Where(x => x.Key.Equals("Global", StringComparison.OrdinalIgnoreCase)).First().Value;
            var addMethodMappingChange = addMethodMapping.CodeChanges.First();
            if (!useTopLevelsStatements)
            {
                addMethodMappingChange = DocumentBuilder.AddLeadingTriviaSpaces(addMethodMappingChange, spaces: 12);
            }

            addMethodMappingChange.Block = string.Format(addMethodMappingChange.Block, mapMethodName);
            var globalChanges = new CodeSnippet[] { addMethodMappingChange };

            //add changes to the root
            if (useTopLevelsStatements)
            {
                newRoot = DocumentBuilder.ApplyChangesToMethod(newRoot, globalChanges) as CompilationUnitSyntax;
            }
            else
            {
                var mainMethod = DocumentBuilder.GetMethodFromSyntaxRoot(newRoot, Main);
                if (mainMethod is not null && mainMethod.Body is not null)
                {
                    var updatedMethod = DocumentBuilder.ApplyChangesToMethod(mainMethod.Body, globalChanges);
                    newRoot = newRoot?.ReplaceNode(mainMethod.Body, updatedMethod);
                }
            }

            //add openapi changes
            if (templateModel.OpenAPI && newRoot is not null)
            {
                var openApiMethodChanges = programCsFile.Methods.Where(x => x.Key.Equals("OpenApi", StringComparison.OrdinalIgnoreCase)).First().Value;
                newRoot = AddOpenApiChanges(newRoot, openApiMethodChanges, useTopLevelsStatements);
            }

            if (docRoot is not null && newRoot is not null)
            {
                //replace root node with all the updates.
                docEditor.ReplaceNode(docRoot, newRoot);
            }

            //write to Program.cs file
            var changedDocument = docEditor.GetChangedDocument();
            var classFileTxt = await changedDocument.GetTextAsync();
            File.WriteAllText(programDocument.FilePath, classFileTxt.ToString());
        }

        private async Task<string?> GetNamespaceFromDocumentAsync(Document endpointsDocument, string className)
        {
            SyntaxTree? syntaxTree = await endpointsDocument.GetSyntaxTreeAsync();
            if (syntaxTree is null)
            {
                return null;
            }
            var root = await syntaxTree.GetRootAsync();
            var classDeclaration = root.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault(c => c.Identifier.ValueText == className);
            var namespaceNode = classDeclaration?.Ancestors().FirstOrDefault(node => node is NamespaceDeclarationSyntax nsDeclarationSyntax || node is FileScopedNamespaceDeclarationSyntax fsDeclarationSyntax);
            return namespaceNode?.ToString();
        }

        private CompilationUnitSyntax? AddOpenApiChanges(CompilationUnitSyntax? newRoot, Method? openApiMethodChanges, bool useTopLevelsStatements)
        {
            if (newRoot is null || openApiMethodChanges is null)
            {
                return null;
            }

            var builderVariable = ProjectModifierHelper.GetBuilderVariableIdentifierTransformation(newRoot.Members);
            if (builderVariable.HasValue)
            {
                (string oldValue, string newValue) = builderVariable.Value;
                CodeSnippet[] filteredChanges = ProjectModifierHelper.UpdateVariables(openApiMethodChanges.CodeChanges, oldValue, newValue);
                if (!useTopLevelsStatements)
                {
                    filteredChanges = DocumentBuilder.AddLeadingTriviaSpaces(filteredChanges, spaces: 12);
                    var mainMethod = DocumentBuilder.GetMethodFromSyntaxRoot(newRoot, Main);
                    var updatedMethod = DocumentBuilder.ApplyChangesToMethod(mainMethod.Body, filteredChanges);
                    if (mainMethod.Body is not null)
                    {
                        newRoot = newRoot.ReplaceNode(mainMethod.Body, updatedMethod);
                    }
                }
                else
                {
                    newRoot = DocumentBuilder.ApplyChangesToMethod(newRoot, filteredChanges) as CompilationUnitSyntax;
                }
            }

            return newRoot;
        }

        //Given CodeAnalysis.Project and ModelType, return CodeAnalysis.Document by reading the latest file from disk.
        //Need CodeAnalysis.Project for AddDocument method.
        internal Document? GetEndpointsDocument(Project project, ModelType type)
        {
            if (project != null && !string.IsNullOrEmpty(project.FilePath) && type != null)
            {
                if (!string.IsNullOrEmpty(type.FullPath))
                {
                    return GetDocumentFromDisk(project, type);
                }
                else
                {
                    string filePath = ValidateAndGetOutputPath(type.Name, project.FilePath) ?? Directory.GetCurrentDirectory();
                    string fileText = string.Format(StaticClassText, type.Name) + "\n{\n}";
                    return project.AddDocument(filePath, fileText, filePath: filePath);
                }
            }

            return null;
        }

        internal Document? GetDocumentFromDisk(Project project, ModelType? type)
        {
            if (project != null && type != null && !string.IsNullOrEmpty(type.FullPath))
            {
                string fileText = File.ReadAllText(type.FullPath);
                if (!string.IsNullOrEmpty(fileText))
                {
                   return project.AddDocument(type.Name, fileText, filePath: type.FullPath);
                }
            }

            return null;
        }

        internal const string Main = nameof(Main);
        internal string StaticClassText = @"
public static class {0}";

        private List<string> AddEndpointsUsings(EndpointsModel templateModel)
        {
            var usings = new List<string>();
            //add usings for DbContext related actins.
            if (!string.IsNullOrEmpty(templateModel.DbContextNamespace))
            {
                usings.Add(templateModel.DbContextNamespace);
            }

            if (!string.IsNullOrEmpty(templateModel.ContextTypeName))
            {
                usings.Add(Scaffolding.Shared.EfConstants.EfUsingName);
            }

            if (templateModel.OpenAPI)
            {
                usings.Add("Microsoft.AspNetCore.OpenApi");
            }

            if (templateModel.UseTypedResults)
            {
                usings.Add("Microsoft.AspNetCore.Http.HttpResults");
            }

            return usings;
        }

        private CodeModifierConfig? GetMinimalApiCodeModifierConfig()
        {
            string jsonText = string.Empty;
            var assembly = Assembly.GetExecutingAssembly();
            var resourceNames = assembly?.GetManifestResourceNames();
            var resourceName = resourceNames?.Where(x => x.EndsWith("minimalApiChanges.json")).FirstOrDefault();
            if (!string.IsNullOrEmpty(resourceName))
            {
                using Stream? stream = assembly?.GetManifestResourceStream(resourceName);
                if (stream is not null)
                {
                    jsonText = new StreamReader(stream).ReadToEnd();
                }
            }
            try
            {
                return JsonSerializer.Deserialize<CodeModifierConfig>(jsonText);
            }
            catch (JsonException)
            {

            }
            return null;
        }

    }
}

