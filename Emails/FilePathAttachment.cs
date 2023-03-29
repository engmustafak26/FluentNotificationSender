using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace FluentNotificationSender.Emails
{
    public sealed class FilePathAttachment : EmailMessageAttachment
    {
        [JsonConstructor]
        public FilePathAttachment(string fileName, string mediaType)
        {
            FileName = fileName;
            MediaType = mediaType;
        }
        public FilePathAttachment(string fileName) : this(fileName, null) { }

        public string FileName { get; private set; }
        public string MediaType { get; private set; }
    }
}
