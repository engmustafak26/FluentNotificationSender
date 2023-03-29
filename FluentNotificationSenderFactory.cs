using FluentNotificationSender.Configurations;
using FluentNotificationSender.Extensions;
using FluentNotificationSender.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentNotificationSender
{
    public static class FluentNotificationSenderFactory
    {
        private static NotificationOptions NotificationOptions { get; set; }

        public static void Configure(Action<NotificationOptions> configure)
        {
            if (configure == null)
                return;

            NotificationOptions options = new NotificationOptions();
            configure(options);
            options.ConfigureAndValidate();
            NotificationOptions = options.DeepClone();
        }

        public static IFluentNotificationService Generate()
        {
            return new FluentNotificationService(NotificationOptions);
        }
    }
}
