﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Net.Mail;
using System.Text;
using NuGet.Services.Entities;
using NuGet.Services.Messaging.Email;
using NuGet.Services.Validation;

namespace NuGetGallery.Infrastructure.Mail.Messages
{
    public class PackageValidationFailedMessage(
        IMessageServiceConfiguration configuration,
        Package package,
        PackageValidationSet validationSet,
        string packageUrl,
        string packageSupportUrl,
        string announcementsUrl,
        string twitterUrl) : MarkdownEmailBuilder
    {
        private readonly IMessageServiceConfiguration _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        private readonly Package _package = package ?? throw new ArgumentNullException(nameof(package));
        private readonly PackageValidationSet _validationSet = validationSet ?? throw new ArgumentNullException(nameof(validationSet));
        private readonly string _packageUrl = packageUrl ?? throw new ArgumentNullException(nameof(packageUrl));
        private readonly string _packageSupportUrl = packageSupportUrl ?? throw new ArgumentNullException(nameof(packageSupportUrl));
        private readonly string _announcementsUrl = announcementsUrl ?? throw new ArgumentNullException(nameof(announcementsUrl));
        private readonly string _twitterUrl = twitterUrl ?? throw new ArgumentNullException(nameof(twitterUrl));

        public override MailAddress Sender => _configuration.GalleryNoReplyAddress;

        public override IEmailRecipients GetRecipients()
        {
            var to = EmailRecipientsHelper.GetAllOwners(_package.PackageRegistration, requireEmailAllowed: false);
            return new EmailRecipients(to);
        }

        public override string GetSubject()
            => $"[{_configuration.GalleryOwner.DisplayName}] Package validation failed - {_package.PackageRegistration.Id} {_package.Version}";

        protected override string GetMarkdownBody()
        {
            var validationIssues = _validationSet.GetValidationIssues();

            var bodyBuilder = new StringBuilder();
            bodyBuilder.Append($@"The package [{_package.PackageRegistration.Id} {_package.Version}]({_packageUrl}) failed validation because of the following reason(s):
");

            foreach (var validationIssue in validationIssues)
            {
                bodyBuilder.Append($@"
- {validationIssue.ToMarkdownString(_announcementsUrl, _twitterUrl)}");
            }

            bodyBuilder.Append($@"

Your package was not published on {_configuration.GalleryOwner.DisplayName} and is not available for consumption.

");

            if (validationIssues.Any(i => i.IssueCode == ValidationIssueCode.Unknown))
            {
                bodyBuilder.Append($"Please [contact support]({_packageSupportUrl}) to help fix your package.");
            }
            else
            {
                var issuePluralString = validationIssues.Count() > 1 ? "all the issues" : "the issue";
                bodyBuilder.Append($"You can reupload your package once you've fixed {issuePluralString} with it.");
            }

            return bodyBuilder.ToString();
        }
    }
}