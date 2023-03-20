using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirebaseAdmin.Messaging;
using FluentNotificationSender.Abstractions;
using FluentNotificationSender.Emails;
using FluentNotificationSender.MobileNotifications;
using FluentNotificationSender.SMS;

namespace FluentNotificationSender.Interfaces
{
    public interface IFluentNotificationService
    {
        IEnumerable<INotificationMethod> GetNotificationMethods(Func<INotificationMethod, bool> where = null);
        Task<FluentNotificationsAggregateResult> SendAsync();
        void SendAsync(bool isFireAndForget);
        IFluentNotificationService WithEmail(EmailVendor emailVendor, EmailMessage message, int? retryCount = null);
        IFluentNotificationService WithEmail(EmailMessage message);

        IFluentNotificationService WithSMS(SMSVendor smsVendor, SMSMessage message, int? retryCount = null);
        IFluentNotificationService WithSMS(SMSMessage message);

        IFluentNotificationService WithMobilePushNotification(MobilePushNotificationVendor mobileVendor, MobileNotificationMessage message, int? retryCount = null);
        IFluentNotificationService WithMobilePushNotification(MobileNotificationMessage message);

        //INotificationService WithMobile(EmailVendor emailVendor, EmailMessage message);
        //INotificationService WithMobile(EmailMessage message);
    }

}

