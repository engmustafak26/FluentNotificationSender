using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using FluentNotificationSender.Abstractions;
using FluentNotificationSender.Extensions;
using FluentNotificationSender.Interfaces;
using FluentNotificationSender.Exceptions;

namespace FluentNotificationSender.Configurations
{
    public class NotificationOptions
    {
        private const string SettingSection = "FluentNotification";
        public NotificationMethodOptions<EmailVendor> EmailOptions { get; internal set; } = new NotificationMethodOptions<EmailVendor>();
        public NotificationMethodOptions<SMSVendor> SMSOptions { get; internal set; } = new NotificationMethodOptions<SMSVendor>();
        public NotificationMethodOptions<MobilePushNotificationVendor> MobileOptions { get; internal set; } = new NotificationMethodOptions<MobilePushNotificationVendor>();
        public AutoRetryAnotherVendor AutoRetryAnotherVendor { get; set; }

        public int? RetryCount { get; internal set; }
        internal IEnumerable<INotificationMethod> GetAllNotificationMethodsCopy(Func<INotificationMethod, bool> where = null)
        {
            var emailVendors = EmailOptions?.Vendors ?? Enumerable.Empty<EmailVendor>();
            var smsVendors = SMSOptions?.Vendors ?? Enumerable.Empty<SMSVendor>();
            var mobileVendors = MobileOptions?.Vendors ?? Enumerable.Empty<MobilePushNotificationVendor>();

            List<INotificationMethod> notifications = new List<INotificationMethod>(emailVendors.Count() + smsVendors.Count() + mobileVendors.Count());
            notifications.AddRange(emailVendors);
            notifications.AddRange(smsVendors);
            notifications.AddRange(mobileVendors);

            notifications = notifications.Where(where ?? (n => true)).ToList();

            foreach (var item in notifications)
            {
                yield return item.DeepClone();
            }
        }

        internal IEnumerable<Vendor> GetAllVendors(Func<Vendor, bool> where = null)
        {
            var emailVendors = EmailOptions?.Vendors ?? Enumerable.Empty<EmailVendor>();
            var smsVendors = SMSOptions?.Vendors ?? Enumerable.Empty<SMSVendor>();
            var mobileVendors = MobileOptions?.Vendors ?? Enumerable.Empty<MobilePushNotificationVendor>();

            List<Vendor> vendors = new List<Vendor>(emailVendors.Count() + smsVendors.Count() + mobileVendors.Count());
            vendors.AddRange(emailVendors);
            vendors.AddRange(smsVendors);
            vendors.AddRange(mobileVendors);

            vendors = vendors.Where(where ?? (n => true)).ToList();

            foreach (var vendor in vendors)
            {
                yield return vendor;
            }
        }

        internal IEnumerable<Vendor<MessagBase>> GetAllVendorsWithMessages(Func<Vendor<MessagBase>, bool> where = null)
        {
            var emailVendors = EmailOptions?.Vendors ?? Enumerable.Empty<EmailVendor>();
            var smsVendors = SMSOptions?.Vendors ?? Enumerable.Empty<SMSVendor>();
            var mobileVendors = MobileOptions?.Vendors ?? Enumerable.Empty<MobilePushNotificationVendor>();

            List<Vendor<MessagBase>> vendors = new List<Vendor<MessagBase>>(emailVendors.Count() + smsVendors.Count() + mobileVendors.Count());
            vendors.AddRange(emailVendors.Select(x => x.ToBase()));
            vendors.AddRange(smsVendors.Select(x => x.ToBase()));
            vendors.AddRange(mobileVendors.Select(x => x.ToBase()));

            vendors = vendors.Where(where ?? (n => true)).ToList();

            foreach (var vendor in vendors)
            {
                yield return vendor;
            }
        }


        internal bool IsGlobalIndexDuplicationExist()
        {
            var emailVendors = EmailOptions?.Vendors ?? Enumerable.Empty<EmailVendor>();
            var smsVendors = SMSOptions?.Vendors ?? Enumerable.Empty<SMSVendor>();
            var mobileVendors = MobileOptions?.Vendors ?? Enumerable.Empty<MobilePushNotificationVendor>();

            List<Vendor> vendors = new List<Vendor>(emailVendors.Count() + smsVendors.Count() + mobileVendors.Count());
            vendors.AddRange(emailVendors);
            vendors.AddRange(smsVendors);
            vendors.AddRange(mobileVendors);
            return vendors.GroupBy(x => x.GlobalIndex).Any(x => x.Count() > 1);
        }
        public NotificationOptions UseSettingFile(IConfiguration configuration, string sectionName = SettingSection)
        {
            var configurationSettings = configuration.GetSection(sectionName).Get<ConfigurationSettings>(o => { o.BindNonPublicProperties = true; });
            configurationSettings.Map(this);
            return this;
        }

        internal void ConfigureAndValidate()
        {

            this.EmailOptions = this.EmailOptions ?? new NotificationMethodOptions<EmailVendor>();
            this.EmailOptions.AutoRetryAnotherVendor = this.EmailOptions.AutoRetryAnotherVendor ?? this.AutoRetryAnotherVendor;
            this.EmailOptions.RetryCount = this.EmailOptions.RetryCount ?? this.RetryCount;
            this.EmailOptions.Vendors.ForEach(x => x.RetryCount = this.EmailOptions.RetryCount);
            this.EmailOptions.SafeAddActiveVendor(this.EmailOptions.Vendors.OrderBy(v => v.UseAsDefault ? 0 : 1).FirstOrDefault());


            this.SMSOptions = this.SMSOptions ?? new NotificationMethodOptions<SMSVendor>();
            this.SMSOptions.AutoRetryAnotherVendor = this.SMSOptions.AutoRetryAnotherVendor ?? this.AutoRetryAnotherVendor;
            this.SMSOptions.RetryCount = this.SMSOptions.RetryCount ?? this.RetryCount;
            this.SMSOptions.Vendors.ForEach(x => x.RetryCount = this.SMSOptions.RetryCount);
            this.SMSOptions.SafeAddActiveVendor(this.SMSOptions.Vendors.OrderBy(v => v.UseAsDefault ? 0 : 1).FirstOrDefault());


            this.MobileOptions = this.MobileOptions ?? new NotificationMethodOptions<MobilePushNotificationVendor>();
            this.MobileOptions.AutoRetryAnotherVendor = this.MobileOptions.AutoRetryAnotherVendor ?? this.AutoRetryAnotherVendor;
            this.MobileOptions.RetryCount = this.MobileOptions.RetryCount ?? this.RetryCount;
            this.MobileOptions.Vendors.ForEach(x => x.RetryCount = this.MobileOptions.RetryCount);
            this.MobileOptions.SafeAddActiveVendor(this.MobileOptions.Vendors.OrderBy(v => v.UseAsDefault ? 0 : 1).FirstOrDefault());

            if (this.IsGlobalIndexDuplicationExist())
            {
                throw new VendorGlobalIndexDuplicationException("Global Index duplication Exist, you must assign unique GlobalIndex for each vendor");
            }
        }


    }
}
