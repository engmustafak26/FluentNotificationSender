using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNotificationSender.Emails;
using FluentNotificationSender.Interfaces;
using FluentNotificationSender.SMS;

namespace FluentNotificationSender.Abstractions
{
    public abstract class SMSVendor : Vendor, ISMSNotificationMethod
    {
        private protected SMSVendor()
        {
        }

        internal abstract override Task<NotificationResult>[] SendAsync();

        [JsonProperty]
        internal List<SMSMessage> Messages { get; set; }
        internal void SafeAdd(SMSMessage message)
        {
            Messages = Messages ?? new List<SMSMessage>();
            Messages.Add(message);
        }


    }
}
