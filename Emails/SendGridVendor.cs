using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNotificationSender.Abstractions;

namespace FluentNotificationSender.Emails
{
    public sealed class SendGridVendor : EmailVendor
    {
    
        internal override Task<NotificationResult>[] SendAsync()
        {
            throw new NotImplementedException();
        }
    }
}
