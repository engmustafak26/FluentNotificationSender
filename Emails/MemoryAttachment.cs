using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FluentNotificationSender.Emails
{
    public sealed class MemoryAttachment : EmailMessageAttachment
    {
        [JsonConstructor]
        public MemoryAttachment(byte[] content, string name, string mediaType)
        {
            Content = content;
            Name = name;
            MediaType = mediaType;
        }

        public MemoryAttachment(byte[] content, string name) : this(content, name, null) { }
        public MemoryAttachment(MemoryStream content, string name) : this(content.ToArray(), name, null) { }
        public MemoryAttachment(MemoryStream content, string name, string mediaType) : this(content.ToArray(), name, mediaType) { }



        public byte[] Content { get; private set; }
        public string Name { get; private set; }
        public string? MediaType { get; private set; }

        internal void ResetContent()
        {
            Content = null;
        }
    }
}
