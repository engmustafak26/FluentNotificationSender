using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using FluentNotificationSender.Abstractions;
using FluentNotificationSender.Extensions;
using FluentNotificationSender.Interfaces;

namespace FluentNotificationSender.Configurations
{
    public class NotificationOptions
    {
        private const string SettingSection = "FluentNotification";
        public NotificationMethodOptions<EmailVendor> EmailOptions { get; internal set; }
        public NotificationMethodOptions<SMSVendor> SMSOptions { get; internal set; }
        public NotificationMethodOptions<MobilePushNotificationVendor> MobileOptions { get; internal set; }
        public int RetryCount { get; internal set; }
        internal IEnumerable<INotificationMethod> GetAllNotificationMethods(Func<INotificationMethod, bool> where = null)
        {
            var emailVendors = EmailOptions?.Vendors ?? Enumerable.Empty<EmailVendor>();
            var smsVendors = SMSOptions?.Vendors ?? Enumerable.Empty<SMSVendor>();
            var mobileVendors = MobileOptions?.Vendors ?? Enumerable.Empty<MobilePushNotificationVendor>();

            List<INotificationMethod> notifications = new List<INotificationMethod>(emailVendors.Count() + smsVendors.Count());
            notifications.AddRange(emailVendors);
            notifications.AddRange(smsVendors);

            notifications = notifications.Where(where ?? (n => true)).ToList();

            foreach (var item in notifications)
            {
                yield return item.DeepClone();
            }
        }
        public NotificationOptions UseSettingFile(IConfiguration configuration, string sectionName = SettingSection)
        {
            var configurationSettings = configuration.GetSection(sectionName).Get<ConfigurationSettings>(o => { o.BindNonPublicProperties = true; });
            configurationSettings.Map(this);
            return this;
        }

    }
}
