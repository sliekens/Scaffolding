// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Spectre.Console.Cli;
using System.Diagnostics;
using System.Collections.Generic;
using Spectre.Console.Flow;
using Microsoft.DotNet.Tools.Scaffold.Commands.Flow.Steps;
using Microsoft.DotNet.Tools.Scaffold.Commands.Services;

namespace Microsoft.DotNet.Tools.Scaffold.Commands.Commands
{
    internal class MinimalApiCommand : BaseCommand<CommandSettings>
    {
        public MinimalApiCommand(IFlowProvider flowProvider) : base(flowProvider)
        { }

        public override async Task<int> ExecuteAsync(CommandContext context, CommandSettings settings)
        {
            //Debugger.Launch();
            //check for base scaffolder settings aka project path for now
            IEnumerable<IFlowStep> flowSteps = new IFlowStep[]
            {
                new ScaffolderPickerFlowStep(),
                new SourceProjectFlowStep(),
                new ModelClassPickerFlowStep("Model Class", string.Empty, string.Empty),
                new ModelClassPickerFlowStep("DbContext Class", "Microsoft.EntityFrameworkCore.DbContext", "DbContext"),
                new MinimalApiExecuteFlowStep()
                // Pick a model class? --> new ModelClassFlowStep()
                // Are you using an existing endpoints class --> new ModelClassFlowStep() 
                // Is this an EF scenario aka with a dbcontext? if yes --> new DbContextFlowStep()
            };

            return await RunFlowAsync(flowSteps, settings, true);
            /*            var minimalApiTemplates = T4TemplateHelper.GetAllMinimalEndpointsT4(AppInfo.ApplicationBasePath, ProjectContext);
                        var minimalApiTemplatePath = minimalApiTemplates.First(x => x.Contains(GetTemplateName(model, existingEndpointsFile: false)));
                        var minimalApiT4Generator = T4TemplateHelper.CreateT4Generator(ServiceProvider, minimalApiTemplatePath);
                        TemplateInvoker templateInvoker = new TemplateInvoker(ServiceProvider);
                        var dictParams = new Dictionary<string, object>()
                                {
                                    { "Model" , templateModel }
                                };

                        var result = templateInvoker.InvokeTemplate(minimalApiT4Generator, dictParams);
                        using (var sourceStream = new MemoryStream(Encoding.UTF8.GetBytes(result)))
                        {
                            //await CodeGeneratorHelper.AddFileHelper(FileSystem, endpointsFilePath, sourceStream);
                        }*/
        }
    }

    internal class MinimalApiCommandSettings : ModelScaffolderSettings
    {
        //[Description("Endpoints class to use. (not file name)")]
        public string? EndpointsClassName { get; set; }

        //[Description("Use this option to enable OpenAPI")]
        public bool OpenApi { get; set; }

        //[Description("Specify the name of the namespace to use for the generated controller")]
        public string? EndpointsNamespace { get; set; }

        //[Description("Flag to not use TypedResults for minimal apis.")]
        public bool NoTypedResults { get; set; }
    }
}
