using System.Collections.Generic;
using System.Linq;
using FluentNotificationSender.SMS;

namespace FluentNotificationSender.Configurations
{
    internal class SMSConfiguration
    {
        public SMSConfiguration()
        {
            Unifonic = new List<UnifonicVendor>();
        }
        public List<UnifonicVendor> Unifonic { get; set; }

        public bool IsSectionExist => Unifonic.Any();

    }
}

