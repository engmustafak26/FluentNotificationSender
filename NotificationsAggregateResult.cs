using System;
using System.Linq;

namespace FluentNotificationSender
{
    public class NotificationsAggregateResult
    {
        public readonly float SuccessRate;
        public readonly float FailRate;
        public readonly int TotalNotifications;
        public readonly DateTime RequestedOn;
        public readonly DateTime ResponseOn;
        public readonly NotificationResult[] NotificationResults;
        public TimeSpan ElapsedTime => ResponseOn.Subtract(RequestedOn);

        public NotificationsAggregateResult(DateTime requestOn, NotificationResult[] notificationResults)
        {
            RequestedOn = requestOn;
            ResponseOn = DateTime.Now;
            TotalNotifications = notificationResults.Length;
            SuccessRate = (float)notificationResults.Count(r => r.IsSuccess) / (notificationResults.Any() ? notificationResults.Count() : 1) * 100;
            FailRate = (float)notificationResults.Count(r => !r.IsSuccess) / (notificationResults.Any() ? notificationResults.Count() : 1) * 100;
            NotificationResults = notificationResults;
        }

    }
}
