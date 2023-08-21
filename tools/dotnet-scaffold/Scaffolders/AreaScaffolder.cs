// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.DotNet.Tools.Scaffold.Scaffolders;
using Spectre.Console.Flow;

namespace Microsoft.DotNet.Tools.Scaffold.Commands
{
    internal class AreaScaffolder : IInternalScaffolder
    {
        public AreaScaffolder(IFlowContext flowContext)
        {
            _flowContext = flowContext ?? throw new ArgumentNullException(nameof(flowContext));
        }

        private int EnsureFolderLayout(string projectPath, string areaName)
        {
            string? applicationBasePath =  Path.GetDirectoryName(projectPath);
            if (string.IsNullOrWhiteSpace(applicationBasePath) || !Directory.Exists(applicationBasePath))
            {
                return -1;
            }

            string? areaBasePath = Path.Combine(applicationBasePath, "Areas");
            if (!Directory.Exists(areaBasePath))
            {
                Directory.CreateDirectory(areaBasePath);
            }

            var areaPath = Path.Combine(areaBasePath, areaName);
            if (!Directory.Exists(areaPath))
            {
                Directory.CreateDirectory(areaPath);
            }

            foreach (var areaFolder in AreaFolders)
            {
                var path = Path.Combine(areaPath, areaFolder);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }

            return 0;
        }

        public async Task<int> ExecuteAsync()
        {
            await Task.Delay(1);
            var areaName = _flowContext.GetValue<string>(Flow.FlowProperties.AreaName);
            var projectPath = _flowContext.GetValue<string>(Flow.FlowProperties.SourceProjectPath);
            if (string.IsNullOrWhiteSpace(areaName) || string.IsNullOrWhiteSpace(projectPath))
            {
                return -1;
                //throw exception
            }

            return EnsureFolderLayout(projectPath, areaName);
        }

        private static readonly string[] AreaFolders = new string[]
        {
            "Controllers",
            "Models",
            "Data",
            "Views"
        };

        private readonly IFlowContext _flowContext;
    }
}
