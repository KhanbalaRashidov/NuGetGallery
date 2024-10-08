﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace NuGetGallery
{
    public class AccessConditionWrapper : IAccessCondition
    {
        private AccessConditionWrapper(string ifNoneMatchETag, string ifMatchETag)
        {
            IfNoneMatchETag = ifNoneMatchETag;
            IfMatchETag = ifMatchETag;
        }

        public string IfNoneMatchETag { get; }

        public string IfMatchETag { get; }

        public static IAccessCondition GenerateEmptyCondition() =>
            new AccessConditionWrapper(
                ifNoneMatchETag: null,
                ifMatchETag: null);

        public static IAccessCondition GenerateIfMatchCondition(string etag) =>
            new AccessConditionWrapper(
                ifNoneMatchETag: null,
                ifMatchETag: etag);

        public static IAccessCondition GenerateIfNoneMatchCondition(string etag) =>
            new AccessConditionWrapper(
                ifNoneMatchETag: etag,
                ifMatchETag: null);

        public static IAccessCondition GenerateIfNotExistsCondition() =>
            new AccessConditionWrapper(
                ifNoneMatchETag: "*",
                ifMatchETag: null);
    }
}
