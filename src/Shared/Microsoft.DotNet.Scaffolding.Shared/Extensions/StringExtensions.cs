using System;
using System.Diagnostics;
using System.IO;

namespace Microsoft.DotNet.Scaffolding.Shared.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Makes sure the string has the trailing character
        /// </summary>
        public static string EnsureTrailingChar(this string s, char ch)
        {
            return s.Length == 0 || s[s.Length - 1] != ch ? s + ch : s;
        }

        /// <summary>
        /// Makes sure the string has a trailing backslash
        /// </summary>
        public static string EnsureTrailingBackslash(this string s)
        {
            return s.EnsureTrailingChar(Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// Helper to make relative paths from physical paths. ie. calculates the relative path from 
        /// basePath to fullPath. Note that if either path is null, fullPath will be returned.
        /// Note that two paths that are equal return ".\".
        /// </summary>
        public static string MakeRelativePath(this string fullPath, string basePath)
        {
            if (string.IsNullOrEmpty(basePath) || string.IsNullOrEmpty(fullPath))
            {
                return fullPath!;
            }

            var separator = Path.DirectorySeparatorChar.ToString();
            var tempBasePath = basePath.EnsureTrailingBackslash();
            var tempFullPath = fullPath.EnsureTrailingBackslash();

            string relativePath = string.Empty;

            while (!string.IsNullOrEmpty(tempBasePath))
            {
                if (tempFullPath.StartsWith(tempBasePath, StringComparison.OrdinalIgnoreCase))
                {
                    // Since we may have added the trailing slash we have to account for that here
                    if (fullPath.Length < tempBasePath.Length)
                    {
                        Debug.Assert(
                            (tempBasePath.Length - fullPath.Length) == 1,
                            "We are at the end. Nothing more to do. Add an empty string to handle case where the paths are equal");
                    }
                    else
                    {
                        relativePath += fullPath.Remove(0, tempBasePath.Length);
                    }

                    // Two equal paths are relative by .\
                    if (string.IsNullOrEmpty(relativePath))
                    {
                        relativePath = "." + Path.DirectorySeparatorChar;
                    }

                    break;
                }
                else
                {
                    tempBasePath = tempBasePath.Remove(tempBasePath.Length - 1);
                    var nLastIndex = tempBasePath.LastIndexOf(separator, StringComparison.OrdinalIgnoreCase);
                    if (-1 != nLastIndex)
                    {
                        tempBasePath = tempBasePath.Remove(nLastIndex + 1);
                        relativePath += "..";
                        relativePath += separator;
                    }
                    else
                    {
                        relativePath = fullPath;
                        break;
                    }
                }
            }

            return relativePath;
        }
    }
}
