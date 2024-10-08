﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text.RegularExpressions;
using NuGet.Services.Entities;
using NuGet.Versioning;

namespace NuGetGallery
{
    public static class NuGetVersionFormatter
    {
        public static string Normalize(string version)
        {
            NuGetVersion parsed;
            if (!NuGetVersion.TryParse(version, out parsed))
            {
                return version;
            }

            return parsed.ToNormalizedString();
        }

        public static string ToFullString(string version)
        {
            NuGetVersion nugetVersion;
            if (NuGetVersion.TryParse(version, out nugetVersion))
            {
                return nugetVersion.ToFullString();
            }
            else
            {
                return version;
            }
        }

        public static string GetNormalizedPackageVersion(Package package)
        {
            if (package == null)
            {
                return string.Empty;
            }

            return string.IsNullOrEmpty(package.NormalizedVersion) ? Normalize(package.Version) : package.NormalizedVersion;
        }
    }

    public static class NuGetVersionExtensions
    {
        private const RegexOptions SemanticVersionRegexFlags = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture;
        private static readonly Regex SemanticVersionRegex = RegexEx.CreateWithTimeout(
            @"^(?<Version>\d+(\s*\.\s*\d+){0,3})(?<Release>-[a-z][0-9a-z-]*)?$",
            SemanticVersionRegexFlags);

        public static string ToNormalizedStringSafe(this NuGetVersion self) =>
            self != null ? self.ToNormalizedString() : string.Empty;

        public static string ToFullStringSafe(this NuGetVersion self) =>
            self != null ? self.ToFullString() : string.Empty;

        public static bool IsValidVersionForLegacyClients(this NuGetVersion self)
        {
            var match = SemanticVersionRegex.Match(self.ToString().Trim());

            return match.Success;
        }
    }
}