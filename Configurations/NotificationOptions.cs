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
        private const string SettingSection = "TechnoWays.Notification";
        public NotificationMethodOptions<EmailVendor> EmailOptions { get; set; }
        public NotificationMethodOptions<SMSVendor> SMSOptions { get; set; }
        internal IEnumerable<INotificationMethod> GetAllNotificationMethods(Func<INotificationMethod, bool> where = null)
        {
            var emailVendors = EmailOptions?.Vendors ?? Enumerable.Empty<EmailVendor>();
            var smsVendors = SMSOptions?.Vendors ?? Enumerable.Empty<SMSVendor>();

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
