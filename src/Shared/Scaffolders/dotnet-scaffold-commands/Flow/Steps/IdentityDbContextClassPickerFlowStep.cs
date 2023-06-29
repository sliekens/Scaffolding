/*// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Spectre.Console.Flow;

namespace Microsoft.DotNet.Tools.Scaffold.Commands.Flow.Steps
{
    internal class IdentityDbContextClassPickerFlowStep : ClassPickerBase, IFlowStep
    {
        public IdentityDbContextClassPickerFlowStep() : base("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext", "IdentityDbContext")
        { }

        public string Id => nameof(DbContextClassPickerFlowStep);

        public string DisplayName => "DbContext Class";

        public ValueTask ResetAsync(IFlowContext context, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public ValueTask<FlowStepResult> RunAsync(IFlowContext context, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public ValueTask<FlowStepResult> ValidateUserInputAsync(IFlowContext context, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
*/
