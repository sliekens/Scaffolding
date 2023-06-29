// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.DotNet.Tools.Scaffold.Commands.Flow
{
    internal static class FlowProperties
    {
        internal const string ScaffolderName = nameof(ScaffolderName);
        internal const string SourceProjectPath = nameof(SourceProjectPath);
        internal const string SourceProjectContext = nameof(SourceProjectContext);
        internal const string SourceProjectWorkspace = nameof(SourceProjectWorkspace);
        internal const string CommandSettings = nameof(CommandSettings);
        internal const string BuildPerformed = nameof(BuildPerformed);
        internal const string ModelClassDisplay = nameof(ModelClassDisplay);
        internal const string ModelClassType = nameof(ModelClassType);
    }
}
