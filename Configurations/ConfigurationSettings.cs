using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNotificationSender.Abstractions;
using FluentNotificationSender.Exceptions;
using FluentNotificationSender.MobileNotifications;

namespace FluentNotificationSender.Configurations
{
    internal class ConfigurationSettings
    {
        public AutoRetryAnotherVendor AutoRetryAnotherVendor { get; set; }
        public int? RetryCount { get; set; }
        public EmailConfiguration Email { get; set; }
        public SMSConfiguration SMS { get; set; }
        public MobilePushNotificationConfiguration Mobile { get; set; }

        internal void Map(NotificationOptions notificationOptions)
        {
            notificationOptions.RetryCount = RetryCount;
            notificationOptions.AutoRetryAnotherVendor = this.AutoRetryAnotherVendor;

            if (Email?.IsSectionExist is true)
            {
                notificationOptions.EmailOptions = notificationOptions.EmailOptions ?? new NotificationMethodOptions<EmailVendor>();
                notificationOptions.EmailOptions.RetryCount = this.Email.RetryCount;
                notificationOptions.EmailOptions.AutoRetryAnotherVendor = this.Email.AutoRetryAnotherVendor;
                notificationOptions.EmailOptions.Vendors.Clear();
                notificationOptions.EmailOptions.Vendors.AddRange(this.Email.Smtp);
                notificationOptions.EmailOptions.Vendors.AddRange(this.Email.SendGrid);
            }
            if (SMS?.IsSectionExist is true)
            {
                notificationOptions.SMSOptions = notificationOptions.SMSOptions ?? new NotificationMethodOptions<SMSVendor>();
                notificationOptions.SMSOptions.RetryCount = this.SMS.RetryCount;
                notificationOptions.SMSOptions.AutoRetryAnotherVendor = this.SMS.AutoRetryAnotherVendor;
                notificationOptions.SMSOptions.Vendors.Clear();
                notificationOptions.SMSOptions.Vendors.AddRange(this.SMS.Unifonic);
                notificationOptions.SMSOptions.Vendors.AddRange(this.SMS.Twilio);
            }

            if (Mobile?.IsSectionExist is true)
            {
                notificationOptions.MobileOptions = notificationOptions.MobileOptions ?? new NotificationMethodOptions<MobilePushNotificationVendor>();
                notificationOptions.MobileOptions.RetryCount = this.Mobile.RetryCount;
                notificationOptions.MobileOptions.AutoRetryAnotherVendor = this.Mobile.AutoRetryAnotherVendor;
                notificationOptions.MobileOptions.Vendors.Clear();
                notificationOptions.MobileOptions.Vendors.AddRange(this.Mobile.Firebase);
            }

        }

    }
}

