using FluentNotificationSender.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentNotificationSender.Emails
{
    public class EmailMessage : MessagBase
    {
        public IEnumerable<string> To { get; private set; }
        public IEnumerable<string> CC { get; private set; }
        public string Subject { get; private set; }
        public string Body { get; private set; }
        public bool IsHtmlBody { get; private set; }
        public EmailMessageAttachment[] Attachments { get; private set; }


        [JsonConstructor]
        public EmailMessage(IEnumerable<string> to, IEnumerable<string> cc, string subject, string body, bool isHtmlBody, EmailMessageAttachment[] attachments)
        {
            To = to;
            CC = cc;
            Subject = subject;
            Body = body;
            Attachments = attachments;
            IsHtmlBody = isHtmlBody;
        }
        public EmailMessage(IEnumerable<string> to, IEnumerable<string> cc, string subject, string body, EmailMessageAttachment[] attachments) : this(to, cc, subject, body, false, attachments)
        { }
        public EmailMessage(IEnumerable<string> to, string subject, string body, EmailMessageAttachment[] attachments) : this(to, null, subject, body, false, attachments)
        { }


        public EmailMessage(IEnumerable<string> to, string subject, string body) : this(to, null, subject, body, false, null) { }
        public EmailMessage(IEnumerable<string> to, string subject, bool isHtmlBody, string body) : this(to, null, subject, body, isHtmlBody, null) { }


        public EmailMessage(string to, string subject, string body, EmailMessageAttachment[] attachments) : this(new string[] { to }, null, subject, body, false, attachments) { }
        public EmailMessage(string to, string subject, string body) : this(new string[] { to }, null, subject, body, false, null) { }


        public EmailMessage(string to, string subject, string body, bool isHtmlBody, EmailMessageAttachment[] attachments) : this(new string[] { to }, null, subject, body, isHtmlBody, attachments) { }

        public EmailMessage(string to, string subject, bool isHtmlBody, string body) : this(new string[] { to }, null, subject, body, isHtmlBody, null) { }

    }
}
