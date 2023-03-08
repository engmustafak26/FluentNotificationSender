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

namespace FluentNotificationSender
{
    internal class NotificationService : INotificationService
    {
        public NotificationService(NotificationOptions options)
        {
            _originalOptions = Options = options;
        }

        [JsonConstructor]
        public NotificationService(NotificationOptions options, bool isFromSerializationCall)
        {
            Options = options;
        }

        private NotificationOptions _originalOptions;

        [JsonProperty]
        private NotificationOptions Options { get; set; }

        public IEnumerable<INotificationMethod> GetNotificationMethods(Func<INotificationMethod, bool> where = null) => Options.GetAllNotificationMethods(where);

        public async Task<NotificationsAggregateResult> SendAsync()
        {
            var requestOn = DateTime.Now;
            var notificationResults = Send();
            await Task.WhenAll(notificationResults).ConfigureAwait(false);

            return new NotificationsAggregateResult(requestOn, notificationResults.Select(r => r.Result).ToArray());
        }

        public void SendAsync(bool isFireAndForget)
        {
            _ = Send();
        }

        private List<Task<NotificationResult>> Send()
        {
            List<Task<NotificationResult>> notificationResults = new List<Task<NotificationResult>>();

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


            if (Options != _originalOptions)
                Options = null;

            return notificationResults;
        }

        public INotificationService WithEmail(EmailVendor emailVendor, EmailMessage message)
        {
            var notification = this.DeepClone();
            emailVendor.SafeAdd(message);
            notification.Options.EmailOptions.SafeAddActiveVendor(emailVendor);
            return notification;
        }

        public INotificationService WithEmail(EmailMessage message)
        {
            var notification = this.DeepClone();
            notification.Options.EmailOptions.ActiveVendors.FirstOrDefault().SafeAdd(message);
            return notification;
        }


        public INotificationService WithSMS(SMSVendor smsVendor, SMSMessage message)
        {
            var notification = this.DeepClone();
            smsVendor.SafeAdd(message);
            notification.Options.SMSOptions.SafeAddActiveVendor(smsVendor);
            return notification;
        }

        public INotificationService WithSMS(SMSMessage message)
        {
            var notification = this.DeepClone();
            notification.Options.SMSOptions.ActiveVendors.FirstOrDefault().SafeAdd(message);
            return notification;
        }

        //public INotificationService WithMobile(EmailVendor emailVendor, EmailMessage message)
        //{
        //    throw new NotImplementedException();
        //}

        //public INotificationService WithMobile(EmailMessage message)
        //{
        //    throw new NotImplementedException();
        //}

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
