using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.DotNet.Scaffolding.Shared.ProjectModel;
using Microsoft.DotNet.Scaffolding.Shared.T4Templating;

namespace Microsoft.DotNet.Tools.Scaffold.Commands.Templating
{
    internal static class T4TemplateHelper
    {
/*        public static IEnumerable<string> GetTemplateFoldersT4(string appBasePath, string baseFolder, IProjectContext projectContext)
        {
            return TemplateFoldersUtilities.GetTemplateFolders(
                containingProject: Constants.ThisAssemblyName,
                applicationBasePath: appBasePath,
                baseFolders: new[] { baseFolder },
                projectContext: projectContext);
        }
*/
        public static IDictionary<string, List<string>> GetAllRazorPagesT4(string appBasePath, IProjectContext projectContext)
        {
            var razorPages = new Dictionary<string, List<string>>();
/*            var razorPageTemplatesFolder = GetTemplateFoldersT4(appBasePath, Path.Combine("T4", "RazorPages"), projectContext)?.FirstOrDefault();
            if (Directory.Exists(razorPageTemplatesFolder))
            {
                var templateFiles = Directory.EnumerateFiles(razorPageTemplatesFolder, "*.tt", SearchOption.AllDirectories);
                foreach (var razorPageTemplateType in RazorPageTemplates)
                {
                    razorPages.Add(razorPageTemplateType, templateFiles.Where(x => x.Contains(razorPageTemplateType, StringComparison.OrdinalIgnoreCase)).ToList());
                }
            }*/
            return razorPages;
        }

        public static IList<string> GetAllMinimalEndpointsT4(string appBasePath, IProjectContext projectContext)
        {
            var minimalEndpointTemplates = new List<string>();
/*            var minimalEndpointTemplatesFolder = GetTemplateFoldersT4(appBasePath, Path.Combine("T4", "MinimalApi"), projectContext)?.FirstOrDefault();
            if (Directory.Exists(minimalEndpointTemplatesFolder))
            {
                minimalEndpointTemplates = Directory.EnumerateFiles(minimalEndpointTemplatesFolder, "*.tt", SearchOption.AllDirectories).ToList();
            }*/
            return minimalEndpointTemplates;
        }

        public static IList<string> RazorPageTemplates = new List<string>()
        {
            "Empty", "Create","List", "Details", "Delete", "Edit"
        };

        public static ITextTransformation? CreateT4Generator(IServiceProvider serviceProvider, string templatePath)
        {
            if (string.IsNullOrEmpty(templatePath))
            {
                return null;
            }

            var host = new TextTemplatingEngineHost(serviceProvider)
            {
                TemplateFile = templatePath
            };

            ITextTransformation? contextTemplate = null;

            string templateName = Path.GetFileNameWithoutExtension(templatePath);
            if (string.IsNullOrEmpty(templateName))
            {
                return null;
            }

            if (templateName.StartsWith("minimalapi", StringComparison.OrdinalIgnoreCase))
            {
                contextTemplate = CreateT4MinimalApiTemplate(host, templateName);
            }

            //contextTemplate.Session = host.CreateSession();

            return contextTemplate;
        }

        private static ITextTransformation? CreateT4MinimalApiTemplate(TextTemplatingEngineHost host, string templateName)
        {
            ITextTransformation? contextTemplate = null;
/*            if (templateName.Equals(nameof(MinimalApiGenerator), StringComparison.OrdinalIgnoreCase))
            {
                contextTemplate = new MinimalApiGenerator { Host = host };
            }
            else if (templateName.Equals(nameof(MinimalApiEfGenerator), StringComparison.OrdinalIgnoreCase))
            {
                contextTemplate = new MinimalApiEfGenerator { Host = host };
            }*/
            return contextTemplate;
        }
    }
}
