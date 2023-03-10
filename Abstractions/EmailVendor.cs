using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNotificationSender.Emails;
using FluentNotificationSender.Interfaces;

namespace FluentNotificationSender.Abstractions
{
    public abstract class EmailVendor : Vendor, IEmailNotificationMethod
    {
        private protected EmailVendor()
        {
        }

        internal abstract override Task<FluentNotificationResult>[] SendAsync();

        [JsonProperty]
        internal List<EmailMessage> Messages { get; set; }
        internal void SafeAdd(EmailMessage message)
        {
            Messages = Messages ?? new List<EmailMessage>();
            Messages.Add(message);
        }


    }
}
