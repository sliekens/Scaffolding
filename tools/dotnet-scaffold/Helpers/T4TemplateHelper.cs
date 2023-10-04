using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.DotNet.Scaffolding.Shared.T4Templating;
using Microsoft.DotNet.Tools.Scaffold.Templates.Endpoints;

namespace Microsoft.DotNet.Tools.Scaffold.Helpers
{
    internal static class T4TemplateHelper
    {
        public static string GetTemplateT4File(string scaffolderFolderName)
        {
            Templates.ScaffoldingT4Templates.TryGetValue(scaffolderFolderName, out var scaffolderTemplate);
            var currFolder = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty);
            while (currFolder is not null)
            {
                var contentFolder = currFolder.GetDirectories().FirstOrDefault(x => x.Name.Equals("Content", StringComparison.OrdinalIgnoreCase));
                if (contentFolder != null)
                {
                    break;
                }

                currFolder = currFolder?.Parent;
            }

            if (string.IsNullOrEmpty(scaffolderTemplate) || currFolder is null)
            {
                return string.Empty;
            }

            var candidatePath = Path.Combine(currFolder.FullName, "content", scaffolderTemplate);
            if (File.Exists(candidatePath))
            {
                return candidatePath;
            }

            return string.Empty;
        }

        public static IList<string> RazorPageTemplates = new List<string>()
        {
            "Empty", "Create","List", "Details", "Delete", "Edit"
        };

        public static ITextTransformation? CreateT4Generator(string templatePath, IServiceProvider? serviceProvider = null)
        {
            if (string.IsNullOrEmpty(templatePath))
            {
                return null;
            }

            var host = new TextTemplatingEngineHost(serviceProvider)
            {
                TemplateFile = templatePath
            };

            string templateName = Path.GetFileNameWithoutExtension(templatePath);
            if (string.IsNullOrEmpty(templateName))
            {
                return null;
            }

            ITextTransformation? contextTemplate = null;
            switch (templateName)
            {
                case "EndpointsGenerator":
                    contextTemplate = new EndpointsGenerator { Host = host };
                    contextTemplate.Session = host.CreateSession();
                    break;
                case "EndpointsEfGenerator":
                    contextTemplate = new EndpointsEfGenerator { Host = host };
                    contextTemplate.Session = host.CreateSession();
                    break;
                default:
                    break;
            }

            return contextTemplate;
        }
    }
}
