﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace NuGetGallery.Operations.Infrastructure
{
    public class JobLog
    {
        private IList<JobLogBlob> _blobs;

        public string JobName { get; private set; }
        public IEnumerable<JobLogBlob> Blobs { get { return _blobs; } }

        public JobLog(string jobName, List<JobLogBlob> blobs)
        {
            JobName = jobName;

            // The null timestamp is the "current" log
            var primary = blobs.Single(b => !b.ArchiveTimestamp.HasValue);

            // The rest should be ordered by descending date
            var rest = blobs
                .Where(b => b.ArchiveTimestamp.HasValue)
                .OrderByDescending(b => b.ArchiveTimestamp.Value);
            _blobs = Enumerable.Concat(new[] { primary }, rest)
                .ToList();
        }

        public IEnumerable<JobLogEntry> OrderedEntries()
        {
            foreach (var logBlob in _blobs)
            {
                // Load the blob and grab the entries
                var entries = LoadEntries(logBlob);
                foreach (var entry in entries)
                {
                    yield return entry;
                }
            }
        }

        private IEnumerable<JobLogEntry> LoadEntries(JobLogBlob logBlob)
        {
            // Download the blob to a temp file
            var temp = Path.GetTempFileName();
            try
            {
                logBlob.Blob.DownloadToFile(temp);

                // Each line is an entry! Read them in reverse though
                foreach (var line in File.ReadAllLines(temp).Reverse())
                {
                    yield return ParseEntry(line);
                }
            }
            finally
            {
                if (File.Exists(temp))
                {
                    File.Delete(temp);
                }
            }
        }

        private static JsonSerializer _serializer = new JsonSerializer()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };
        private JobLogEntry ParseEntry(string line)
        {
            using (var r = new StringReader(line))
            using (var reader = new JsonTextReader(r))
            {
                return _serializer.Deserialize<JobLogEntry>(reader);
            }
        }
    }
}
