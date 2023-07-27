// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.DotNet.Tools.Scaffold.Scaffolders;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Flow;

namespace Microsoft.DotNet.Tools.Scaffold.Commands
{
    internal class UninstallScaffolder : IInternalScaffolder
    {
        //private readonly IToolService _toolService;
        private readonly IFlowContext _flowContext;

        public UninstallScaffolder(IFlowContext flowContext)
        {
            _flowContext = flowContext;
        }

        public async Task<int> ExecuteAsync()
        {
            await Task.Delay(1);
            return -1;
        }
    }
}

