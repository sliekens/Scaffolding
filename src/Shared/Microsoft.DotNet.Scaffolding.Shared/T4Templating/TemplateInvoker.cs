using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Microsoft.DotNet.Scaffolding.Shared.T4Templating
{
    /// <summary>
    /// Contains useful helper functions for running visual studio text transformation.
    /// For internal microsoft use only. Use <see cref="ITemplateInvoker"/>
    /// in custom code generators.
    /// </summary>
    public class TemplateInvoker : ITemplateInvoker
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="serviceProvider"></param>
        public TemplateInvoker(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <summary>
        /// Executes a code generator template to generate the code.
        /// </summary>
        /// <param name="template">ITextTransformation template object</param>
        /// <param name="templateParameters">Parameters for the template.
        /// These parameters can be accessed in text template using a parameter directive.
        /// The values passed in must be either serializable or 
        /// extend <see cref="MarshalByRefObject"/> type.</param>
        /// <returns>Generated code if there were no processing errors. Throws 
        /// <see cref="InvalidOperationException" /> otherwise.
        /// </returns>
        public string InvokeTemplate(ITextTransformation template, IDictionary<string, object> templateParameters)
        {
            foreach (var param in templateParameters)
            {
                template.Session.Add(param.Key, param.Value);
            }

            string generatedCode = string.Empty;
            if (template != null)
            {
                template.Initialize();
                generatedCode = ProcessTemplate(template);
            }
            return generatedCode;
        }

        private string ProcessTemplate(ITextTransformation transformation)
        {
            var output = transformation.TransformText();

            foreach (CompilerError error in transformation.Errors)
            {
                //_reporter.Write(error);
            }

            if (transformation.Errors.HasErrors)
            {
                //throw new OperationException(DesignStrings.ErrorGeneratingOutput(transformation.GetType().Name));
            }

            return output;
        }

    }
}
