using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNotificationSender.Abstractions;
using FluentNotificationSender.Configurations;
using FluentNotificationSender.Emails;
using FluentNotificationSender.Extensions;
using FluentNotificationSender.Interfaces;
using FluentNotificationSender.SMS;
using FirebaseAdmin.Messaging;

namespace FluentNotificationSender
{
    internal class FluentNotificationService : IFluentNotificationService
    {
        public FluentNotificationService(NotificationOptions options)
        {
            _originalOptions = Options = options;
        }

        [JsonConstructor]
        public FluentNotificationService(NotificationOptions options, bool isFromSerializationCall)
        {
            Options = options;
        }

        private NotificationOptions _originalOptions;

        [JsonProperty]
        private NotificationOptions Options { get; set; }

        public IEnumerable<INotificationMethod> GetNotificationMethods(Func<INotificationMethod, bool> where = null) => Options.GetAllNotificationMethods(where);

        public async Task<FluentNotificationsAggregateResult> SendAsync()
        {
            var requestOn = DateTime.Now;
            var notificationResults = Send();
            await Task.WhenAll(notificationResults).ConfigureAwait(false);

            return new FluentNotificationsAggregateResult(requestOn, notificationResults.Select(r => r.Result).ToArray());
        }

        public void SendAsync(bool isFireAndForget)
        {
            _ = Send();
        }

        private List<Task<FluentNotificationResult>> Send()
        {
            List<Task<FluentNotificationResult>> notificationResults = new List<Task<FluentNotificationResult>>();

            Options.EmailOptions?.ActiveVendors?.ForEach(active =>
            {
                if (active.Messages?.Count > 0)
                    notificationResults.AddRange(active.SendAsync());
            });

            Options.SMSOptions?.ActiveVendors?.ForEach(active =>
            {
                if (active.Messages?.Count > 0)
                    notificationResults.AddRange(active.SendAsync());
            });


            Options.MobileOptions?.ActiveVendors?.ForEach(active =>
            {
                if (active.Messages?.Count > 0)
                    notificationResults.AddRange(active.SendAsync());
            });


            if (Options != _originalOptions)
                Options = null;

            return notificationResults;
        }

        public IFluentNotificationService WithEmail(EmailVendor emailVendor, EmailMessage message)
        {
            var notification = this.DeepClone();
            emailVendor.SafeAdd(message);
            notification?.Options?.EmailOptions?.SafeAddActiveVendor(emailVendor);
            return notification;
        }

        public IFluentNotificationService WithEmail(EmailMessage message)
        {
            var notification = this.DeepClone();
            notification?.Options?.EmailOptions?.ActiveVendors?.FirstOrDefault()?.SafeAdd(message);
            return notification;
        }


        public IFluentNotificationService WithSMS(SMSVendor smsVendor, SMSMessage message)
        {
            var notification = this.DeepClone();
            smsVendor.SafeAdd(message);
            notification?.Options?.SMSOptions?.SafeAddActiveVendor(smsVendor);
            return notification;
        }

        public IFluentNotificationService WithSMS(SMSMessage message)
        {
            var notification = this.DeepClone();
            notification?.Options?.SMSOptions?.ActiveVendors?.FirstOrDefault()?.SafeAdd(message);
            return notification;
        }

        public IFluentNotificationService WithMobilePushNotification(MobilePushNotificationVendor mobileVendor, Message message)
        {
            var notification = this.DeepClone();
            mobileVendor.SafeAdd(message);
            notification?.Options?.MobileOptions?.SafeAddActiveVendor(mobileVendor);
            return notification;
        }

        public IFluentNotificationService WithMobilePushNotification(Message message)
        {
            var notification = this.DeepClone();
            notification?.Options?.MobileOptions?.ActiveVendors?.FirstOrDefault()?.SafeAdd(message);
            return notification;
        }

        //public INotificationService WithWeb(EmailVendor emailVendor, EmailMessage message)
        //{
        //    throw new NotImplementedException();
        //}

        //public INotificationService WithWeb(EmailMessage message)
        //{
        //    throw new NotImplementedException();
        //}


    }
}
