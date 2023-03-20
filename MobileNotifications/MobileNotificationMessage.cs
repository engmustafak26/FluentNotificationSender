using FirebaseAdmin.Messaging;
using FluentNotificationSender.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace FluentNotificationSender.MobileNotifications
{
    public sealed class MobileNotificationMessage : MessagBase
    {
        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonIgnore]
        public string Topic { get; set; }

        [JsonProperty("condition")]
        public string Condition { get; set; }

        [JsonProperty("data")]
        public IReadOnlyDictionary<string, string> Data { get; set; }

        [JsonProperty("notification")]
        public Notification Notification { get; set; }

        [JsonProperty("android")]
        public AndroidConfig Android { get; set; }

        [JsonProperty("webpush")]
        public WebpushConfig Webpush { get; set; }

        [JsonProperty("apns")]
        public ApnsConfig Apns { get; set; }


        [JsonProperty("fcm_options")]
        public FcmOptions FcmOptions { get; set; }

        [JsonProperty("topic")]
        private string UnprefixedTopic
        {
            get
            {
                if (Topic != null && Topic.StartsWith("/topics/"))
                {
                    return Topic.Substring("/topics/".Length);
                }

                return Topic;
            }
            set
            {
                Topic = value;
            }
        }



    }
}
