using System.Collections.Generic;
using System.Linq;
using FluentNotificationSender.Emails;

namespace FluentNotificationSender.Configurations
{
    internal class EmailConfiguration
    {
        public EmailConfiguration()
        {
            Smtp = new List<SmtpVendor>();
            SendGrid = new List<SendGridVendor>();
        }
        public AutoRetryAnotherVendor AutoRetryAnotherVendor { get; set; }
        public int? RetryCount { get; set; }
        public List<SmtpVendor> Smtp { get; set; }
        public List<SendGridVendor> SendGrid { get; set; }

        public bool IsSectionExist => Smtp.Any() || SendGrid.Any();

    }
}

