// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.DotNet.Scaffolding.Shared.Cli.Utils;
using Spectre.Console.Flow;

namespace Microsoft.DotNet.Tools.Scaffold.Scaffolders
{
    internal class RazorPageScaffolder : IInternalScaffolder
    {
        public RazorPageScaffolder(IFlowContext flowContext)
        {
            _flowContext = flowContext ?? throw new ArgumentNullException(nameof(flowContext));
            _commandLineString = new StringBuilder($"dotnet scaffold razorpage ");
        }

        public async Task<int> ExecuteAsync()
        {
            await Task.Delay(1);
            var projectFilePath = _flowContext.GetValue<string>(Flow.FlowProperties.SourceProjectPath);
            var templateName = _flowContext.GetValue<string>(Flow.FlowProperties.RazorPageScaffolderTemplate);

            //arguments for `dotnet new page`
            if (string.IsNullOrEmpty(projectFilePath) ||
                string.IsNullOrEmpty(templateName))
            {
                //need a fugging name
                return -1;
            }

            switch(templateName)
            {
                case "Razor Pages - Empty":
                    ExecuteEmptyRazorPages(projectFilePath);
                    break;
            }

            return 0;
        }

        private int ExecuteEmptyRazorPages(string projectFilePath)
        {
            var projectFolderPath = Path.GetDirectoryName(projectFilePath);
            var razorPageName = _flowContext.GetValue<string>(Flow.FlowProperties.RazorPageName);

            if (string.IsNullOrEmpty(razorPageName) ||  string.IsNullOrEmpty(projectFolderPath))
            {
                return -1;
            }

            var additionalArgs = new List<string>()
            {
                "page",
                "--name",
                razorPageName,
                "--output",
                projectFolderPath
            };

            return DotnetCommands.ExecuteDotnetNew(projectFilePath, additionalArgs, new Scaffolding.Shared.ConsoleLogger());   
        }

        private readonly IFlowContext _flowContext;
        private readonly StringBuilder _commandLineString;
    }
}
