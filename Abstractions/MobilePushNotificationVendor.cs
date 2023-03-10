using FirebaseAdmin.Messaging;
using FluentNotificationSender.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FluentNotificationSender.Abstractions
{
    public abstract class MobilePushNotificationVendor : Vendor, IMobileNotificationMethod
    {
        private protected MobilePushNotificationVendor()
        {
        }

        internal abstract override Task<FluentNotificationResult>[] SendAsync();

        [JsonProperty]
        internal List<Message> Messages { get; set; }
        internal void SafeAdd(Message message)
        {
            Messages = Messages ?? new List<Message>();
            Messages.Add(message);
        }

    }
}
