// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Microsoft.DotNet.Tools.Scaffold
{
    public static class ScaffoldTargetsInstaller
    {
        /// <summary>
        /// Check given the project
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="objFolder"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool EnsureTargetImported(string projectName, string objFolder)
        {
            if (string.IsNullOrEmpty(projectName) || string.IsNullOrEmpty(objFolder))
            {
                return false;
            }

            const string ToolsImportTargetsName = "Imports.targets";
            // Create the directory structure if it doesn't exist.
            Directory.CreateDirectory(objFolder);

            var fileName = $"{projectName}.codegeneration.targets";
            var importingTargetFilePath = Path.Combine(objFolder, fileName);

            if (File.Exists(importingTargetFilePath))
            {
                return true;
            }

            var toolType = typeof(ScaffoldTargetsInstaller);
            var toolAssembly = toolType.GetTypeInfo().Assembly;
            var toolNamespace = toolType.Namespace;
            var toolImportTargetsResourceName = "Microsoft.DotNet.Tools.Scaffold.Imports.targets";
            var toolImportTargetsResourceName1 = $"{toolNamespace}.{ToolsImportTargetsName}";

            using var stream = toolAssembly.GetManifestResourceStream(toolImportTargetsResourceName);
            if (stream is not null)
            {
                var targetBytes = new byte[stream.Length];
                stream.Read(targetBytes, 0, targetBytes.Length);
                File.WriteAllBytes(importingTargetFilePath, targetBytes);
                return true;
            }

            return false;
        }

        internal static string GetTargetsLocation()
        {
            const string build = "build";
            var assembly = typeof(Program).GetTypeInfo().Assembly;
            var path = Path.GetDirectoryName(assembly.Location) ?? string.Empty;
            // Crawl up from assembly location till we find 'build' directory.
            do
            {
                if (Directory.EnumerateDirectories(path, build, SearchOption.TopDirectoryOnly).Any())
                {
                    return path;
                }

                path = Path.GetDirectoryName(path);
            } while (path != null);

            throw new DirectoryNotFoundException("Targets file not found");
        }
    }
}
