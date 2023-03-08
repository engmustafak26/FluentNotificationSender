﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNotificationSender.Abstractions;

namespace FluentNotificationSender.Configurations
{
    internal class ConfigurationSettings
    {
        public EmailConfiguration Email { get; set; }
        public SMSConfiguration SMS { get; set; }

        internal void Map(NotificationOptions notificationOptions)
        {
            if (Email?.IsSectionExist is true)
            {
                notificationOptions.EmailOptions = notificationOptions.EmailOptions ?? new NotificationMethodOptions<EmailVendor>();
                notificationOptions.EmailOptions.Vendors.Clear();
                notificationOptions.EmailOptions.Vendors.AddRange(this.Email.Smtp);
                notificationOptions.EmailOptions.Vendors.AddRange(this.Email.SendGrid);
                notificationOptions.EmailOptions.SafeAddActiveVendor(notificationOptions.EmailOptions.Vendors.OrderBy(v => v.UseAsDefault ? 0 : 1).FirstOrDefault());
            }
            if (SMS?.IsSectionExist is true)
            {
                notificationOptions.SMSOptions = notificationOptions.SMSOptions ?? new NotificationMethodOptions<SMSVendor>();
                notificationOptions.SMSOptions.Vendors.Clear();
                notificationOptions.SMSOptions.Vendors.AddRange(this.SMS.Unifonic);
                notificationOptions.SMSOptions.SafeAddActiveVendor(notificationOptions.SMSOptions.Vendors.OrderBy(v => v.UseAsDefault ? 0 : 1).FirstOrDefault());
            }

        }



    }
}

