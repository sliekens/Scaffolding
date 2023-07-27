// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading.Tasks;
using Microsoft.DotNet.Tools.Scaffold.Scaffolders;
using Microsoft.DotNet.Tools.Scaffold.Services;

namespace Microsoft.DotNet.Tools.Scaffold.Commands
{
    internal class ExternalScaffolder : IInternalScaffolder
    {
        public ExternalScaffolder(IToolService toolService)
        {
            _toolService = toolService ?? throw new ArgumentNullException(nameof(toolService));
        }

/*        public override int Execute([NotNull] CommandContext context, [NotNull] CommandSettings settings)
        {
            var toolInfo = new ToolInfo();// _packageInfoService.GetToolInfo(context.Name);
            if (toolInfo is null)
            {
                return -1;
                //throw a fit
            }

            var exeName = toolInfo.ToolExeName;
            if (!Path.HasExtension(exeName) || Path.GetExtension(exeName).ToLower() != ".exe")
            {
                exeName += ".exe";
            }
            *//*
            var packagesFolder = ""; Path.Combine(_packageInfoService.DotnetScaffolderFolder, "packages");

            var exePath = ""; FileSystemHelper.GetFirstFilePath(packagesFolder, exeName);
            DotnetCommands.InvokeExternalCommand(exePath, new List<string>());
            *//*
            return 0;
        }*/

        public async Task<int> ExecuteAsync()
        {
            await Task.Delay(1);
            throw new NotImplementedException();
        }

        private IToolService _toolService;
    }
}
