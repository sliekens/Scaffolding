﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.DotNet.Cli.Utils;
using Newtonsoft.Json;
using NuGet.Frameworks;

namespace Microsoft.Extensions.ProjectModel
{
    public class MsBuildProjectContextBuilder
    {
        private string _projectPath;
        private NuGetFramework _targetFramework;

        public MsBuildProjectContextBuilder(string projectPath, NuGetFramework targetFramework)
        {
            if (string.IsNullOrEmpty(projectPath))
            {
                throw new ArgumentNullException(nameof(projectPath));
            }

            _projectPath = projectPath;
            _targetFramework = targetFramework ?? FrameworkConstants.CommonFrameworks.NetCoreApp10;
        }

        public IProjectContext Build()
        {
            var errors = new List<string>();
            var output = new List<string>();
            var tmpFile = Path.GetTempFileName();
            var result = Command.Create("dotnet",
                new string[] 
                {
                    "msbuild",
                    _projectPath,
                    $"/t:EvaluateProjectInfoForCodeGeneration", 
                    $"/p:TargetFramework={_targetFramework.GetShortFolderName()};OutputFile={tmpFile}"
                })
                .OnErrorLine(e => errors.Add(e))
                .OnOutputLine(o => output.Add(o))
                .Execute();

            if (result.ExitCode != 0)
            {
                throw CreateProjectContextCreationFailedException(_projectPath, errors);
            }
            try
            {
                var info = File.ReadAllText(tmpFile);

                var buildContext = JsonConvert.DeserializeObject<MsBuildProjectContext>(info);

                return buildContext;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to read the BuildContext information.", ex);
            }
        }

        private Exception CreateProjectContextCreationFailedException(string _projectPath, List<string> errors)
        {
            var errorMsg = $"Failed to get Project Context for {_projectPath}.";
            if (errors != null)
            {
                errorMsg += $"{Environment.NewLine} { string.Join(Environment.NewLine, errors)} ";
            }

            return new InvalidOperationException(errorMsg);
        }
    }
}
