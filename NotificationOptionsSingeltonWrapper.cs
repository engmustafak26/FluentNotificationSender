using FluentNotificationSender.Configurations;
using System;
using System.Collections.Generic;
using System.Text;

namespace FluentNotificationSender
{
    internal static class NotificationOptionsSingeltonWrapper
    {
        internal static NotificationOptions NotificationOptions { get; private set; }
        internal static void SetNotificationOptions(NotificationOptions notificationOptions)
        {
            NotificationOptions = notificationOptions;
        }
    }
}
