// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Extensions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Razor;
using Microsoft.VisualStudio.Web.CodeGeneration.Templating.Compilation;

namespace Microsoft.VisualStudio.Web.CodeGeneration.Templating
{
    //Todo: Make this internal and expose as a sevice
    public class RazorTemplating : ITemplating
    {
        private ICompilationService _compilationService;

        public RazorTemplating(ICompilationService compilationService)
        {
            _compilationService = compilationService ?? throw new ArgumentNullException(nameof(compilationService));
        }

        public async Task<TemplateResult> RunTemplateAsync(string content,
            dynamic templateModel)
        {
            var assemblyPath = Assembly.GetEntryAssembly().Location;
            var dotnetToolsPath = string.Empty;
            if (!string.IsNullOrEmpty(assemblyPath))
            {
                dotnetToolsPath = Path.GetFullPath(Path.GetDirectoryName(assemblyPath));
            }
            dotnetToolsPath = Directory.GetCurrentDirectory();
            // Don't care about the RazorProject as we already have the content of the .cshtml file 
            // and don't need to deal with imports.
            var fileSystem = RazorProjectFileSystem.Create(dotnetToolsPath);
            var projectEngine = RazorProjectEngine.Create(RazorConfiguration.Default, fileSystem, (builder) =>
            {
                builder.SetCSharpLanguageVersion(LanguageVersion.CSharp10);

                SectionDirective.Register(builder);

                builder.AddTargetExtension(new TemplateTargetExtension()
                {
                    TemplateTypeName = "global::Microsoft.AspNetCore.Mvc.Razor.HelperResult",
                });

                builder.AddDefaultImports(@"
                    @using System
                    @using System.Threading.Tasks
                    ");
            });

            var templateItem = new TemplateRazorProjectItem(content);
            var codeDocument = projectEngine.Process(templateItem);
            var generatorResults = codeDocument.GetCSharpDocument();
            var docString = generatorResults.ToString();
            var docString2 = codeDocument.Source.ToString();
            var items = codeDocument.Items;

            if (generatorResults.Diagnostics.Any())
            {
                var messages = generatorResults.Diagnostics.Select(d => d.GetMessage());
                return new TemplateResult()
                {
                    GeneratedText = string.Empty,
                    ProcessingException = new TemplateProcessingException(messages, generatorResults.GeneratedCode)
                };
            }

            var templateResult = _compilationService.Compile(generatorResults.GeneratedCode);
            if (templateResult.Messages.Any())
            {
                return new TemplateResult()
                {
                    GeneratedText = string.Empty,
                    ProcessingException = new TemplateProcessingException(templateResult.Messages, generatorResults.GeneratedCode)
                };
            }

            var compiledObject = Activator.CreateInstance(templateResult.CompiledType);

            var result = string.Empty;
            if (compiledObject is RazorTemplateBase razorTemplate)
            {
                razorTemplate.Model = templateModel;
                //ToDo: If there are errors executing the code, they are missed here.
                result = await razorTemplate.ExecuteTemplate();
            }

            return new TemplateResult()
            {
                GeneratedText = result,
                ProcessingException = null
            };

        }
    }
}
