using System.Collections.Generic;
using System.Linq;
using FluentNotificationSender.MobileNotifications;
using FluentNotificationSender.SMS;

namespace FluentNotificationSender.Configurations
{
    internal class MobilePushNotificationConfiguration
    {
        public MobilePushNotificationConfiguration()
        {
            Firebase = new List<FirebaseVendor>();
        }
        public AutoRetryAnotherVendor AutoRetryAnotherVendor { get; set; }

        public int? RetryCount { get; set; }
        public List<FirebaseVendor> Firebase { get; set; }

        public bool IsSectionExist => Firebase.Any();

    }
}

