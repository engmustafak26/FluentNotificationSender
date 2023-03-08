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
    public interface INotificationService
    {
        IEnumerable<INotificationMethod> GetNotificationMethods(Func<INotificationMethod, bool> where = null);
        Task<NotificationsAggregateResult> SendAsync();
        void SendAsync(bool isFireAndForget);
        INotificationService WithEmail(EmailVendor emailVendor, EmailMessage message);
        INotificationService WithEmail(EmailMessage message);

        INotificationService WithSMS(SMSVendor smsVendor, SMSMessage message);
        INotificationService WithSMS(SMSMessage message);

        //INotificationService WithWeb(EmailVendor emailVendor, EmailMessage message);
        //INotificationService WithWeb(EmailMessage message);

        //INotificationService WithMobile(EmailVendor emailVendor, EmailMessage message);
        //INotificationService WithMobile(EmailMessage message);
    }

}

