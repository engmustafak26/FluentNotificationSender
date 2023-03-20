using FluentNotificationSender.Abstractions;
using FluentNotificationSender.Emails;
using FluentNotificationSender.Extensions;
using FluentNotificationSender.MobileNotifications;
using FluentNotificationSender.SMS;
using System;
using System.Linq;

namespace FluentNotificationSender
{
    public sealed class FluentNotificationsAggregateResult
    {
        public float SuccessRate { get; private set; }
        public float FailRate { get; private set; }
        public int SuccessCount { get; private set; }
        public int FailCount { get; private set; }
        public int TotalCount { get; private set; }
        public int SuccessNotificationsSent { get; private set; }
        public int FailNotificationsSent { get; private set; }
        public int TotalNotificationsSent { get; private set; }

        public DateTime RequestedOn { get; private set; }
        public DateTime ResponseOn { get; private set; }
        public TimeSpan ElapsedTime => ResponseOn.Subtract(RequestedOn);
        public FluentNotificationResult[] NotificationResults { get; private set; }

        public FluentNotificationsAggregateResult(DateTime requestOn, FluentNotificationResult[] notificationResults)
        {
            RequestedOn = requestOn;
            ResponseOn = DateTime.Now;
            TotalCount = notificationResults.Length;
            NotificationResults = notificationResults;
            TotalNotificationsSent = notificationResults.Length;
            DoStatics();
        }
        public void AdjustNotificationResult()
        {
            ResponseOn = DateTime.Now;
            TotalNotificationsSent = 0;
            NotificationResults.ForEach(x =>
            {
                FluentNotificationResult lastResultBase = null;
                if (x.VendorInfo is Vendor<EmailMessage> emailVendor)
                {
                    emailVendor.Messages
                               .SelectMany(x => x.Results)
                               .ToArray()
                               .ForEach(x =>
                               {
                                   x.VendorInfo.SupressSensitiveInfo();
                                   x.VendorInfo.ClearMessages();
                                   SuccessNotificationsSent += x.IsSuccess ? 1 : 0;
                                   TotalNotificationsSent++;
                               });
                    lastResultBase = emailVendor.Messages.LastOrDefault().Results.LastOrDefault();
                }
                else if (x.VendorInfo is Vendor<SMSMessage> smsVendor)
                {
                    smsVendor.Messages
                            .SelectMany(x => x.Results)
                            .ToArray()
                            .ForEach(x =>
                            {
                                x.VendorInfo.SupressSensitiveInfo();
                                x.VendorInfo.ClearMessages();
                                SuccessNotificationsSent += x.IsSuccess ? 1 : 0;
                                TotalNotificationsSent++;
                            });
                    lastResultBase = smsVendor.Messages.LastOrDefault().Results.LastOrDefault();
                }
                else if (x.VendorInfo is Vendor<MobileNotificationMessage> mobileVendor)
                {
                    mobileVendor.Messages
                                .SelectMany(x => x.Results)
                                .ToArray()
                                .ForEach(x =>
                                {
                                    x.VendorInfo.SupressSensitiveInfo();
                                    x.VendorInfo.ClearMessages();
                                    SuccessNotificationsSent += x.IsSuccess ? 1 : 0;
                                    TotalNotificationsSent++;
                                });
                    lastResultBase = mobileVendor.Messages.LastOrDefault().Results.LastOrDefault();
                }

                x.IsSuccess = lastResultBase.IsSuccess;
                x.ResponseOn = lastResultBase.ResponseOn;
                x.ResponseMessage = lastResultBase.ResponseMessage;
                x.Exception = lastResultBase.Exception;


            });
            FailNotificationsSent = TotalNotificationsSent - SuccessNotificationsSent;
            DoStatics();
            var groupedResult = NotificationResults.GroupBy(x => x.VendorInfo.GlobalIndex);
        }

        void DoStatics()
        {
            SuccessCount = NotificationResults.Count(r => r.IsSuccess);
            FailCount = NotificationResults.Length - SuccessCount;
            SuccessRate = (float)SuccessCount / (NotificationResults.Any() ? TotalCount : 1) * 100;
            FailRate = (float)FailCount / (NotificationResults.Any() ? TotalCount : 1) * 100;
        }
    }
}
