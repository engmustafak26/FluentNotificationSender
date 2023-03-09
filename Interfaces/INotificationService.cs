using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNotificationSender.Abstractions;
using FluentNotificationSender.Emails;
using FluentNotificationSender.SMS;

namespace FluentNotificationSender.Interfaces
{
    public interface IFluentNotificationService
    {
        IEnumerable<INotificationMethod> GetNotificationMethods(Func<INotificationMethod, bool> where = null);
        Task<NotificationsAggregateResult> SendAsync();
        void SendAsync(bool isFireAndForget);
        IFluentNotificationService WithEmail(EmailVendor emailVendor, EmailMessage message);
        IFluentNotificationService WithEmail(EmailMessage message);

        IFluentNotificationService WithSMS(SMSVendor smsVendor, SMSMessage message);
        IFluentNotificationService WithSMS(SMSMessage message);

        //INotificationService WithWeb(EmailVendor emailVendor, EmailMessage message);
        //INotificationService WithWeb(EmailMessage message);

        //INotificationService WithMobile(EmailVendor emailVendor, EmailMessage message);
        //INotificationService WithMobile(EmailMessage message);
    }

}

