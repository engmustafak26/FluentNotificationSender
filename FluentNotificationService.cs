using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNotificationSender.Abstractions;
using FluentNotificationSender.Configurations;
using FluentNotificationSender.Emails;
using FluentNotificationSender.Extensions;
using FluentNotificationSender.Interfaces;
using FluentNotificationSender.SMS;
using FirebaseAdmin.Messaging;
using FluentNotificationSender.MobileNotifications;
using System.Reflection;
using FluentNotificationSender.Helpers;
using Microsoft.Extensions.Options;

namespace FluentNotificationSender
{
    internal class FluentNotificationService : IFluentNotificationService
    {
        public FluentNotificationService(NotificationOptions options)
        {
            _originalOptions = Options = options;
        }

        [JsonConstructor]
        public FluentNotificationService(NotificationOptions options, bool isFromSerializationCall)
        {
            Options = options;
        }

        private NotificationOptions _originalOptions;

        [JsonProperty]
        private NotificationOptions Options { get; set; }

        public IEnumerable<INotificationMethod> GetNotificationMethods(Func<INotificationMethod, bool> where = null) => Options.GetAllNotificationMethodsCopy(where);

        public async Task<FluentNotificationsAggregateResult> SendAsync()
        {

            var requestOn = DateTime.Now;
            var notificationResultsTasks = Send();
            await Task.WhenAll(notificationResultsTasks).ConfigureAwait(false);

            var aggregateReuslt = new FluentNotificationsAggregateResult(requestOn, notificationResultsTasks.Select(r => r.Result).ToArray());
            var results = aggregateReuslt;
            while (results.FailCount > 0)
            {
                var requestOnL = DateTime.Now;
                var notificationResultsTasksL = Send();
                await Task.WhenAll(notificationResultsTasksL).ConfigureAwait(false);
                results = new FluentNotificationsAggregateResult(requestOnL, notificationResultsTasksL.Select(r => r.Result).ToArray());
            }

            this.SetActiveVendorToWorkingOne();
            if (Options != _originalOptions)
                Options = null;


            aggregateReuslt.AdjustNotificationResult();
            return aggregateReuslt;
        }

        public void SendAsync(bool isFireAndForget)
        {
            _ = Send();
        }

        private List<Task<FluentNotificationResult>> Send()
        {
            List<Task<FluentNotificationResult>> notificationResults = new List<Task<FluentNotificationResult>>();

            Options.EmailOptions?.ActiveVendors?.ForEach(active =>
            {
                if (active.Messages?.Count > 0)
                {
                    var vendor = GetCandidateVendorOrReturnSelf(active, Options.EmailOptions.AutoRetryAnotherVendor);
                    notificationResults.AddRange(vendor.SendAsync());

                }
            });

            Options.SMSOptions?.ActiveVendors?.ForEach(active =>
            {
                if (active.Messages?.Count > 0)
                {
                    var vendor = GetCandidateVendorOrReturnSelf(active, Options.SMSOptions.AutoRetryAnotherVendor);
                    notificationResults.AddRange(vendor.SendAsync());

                }
            });


            Options.MobileOptions?.ActiveVendors?.ForEach(active =>
            {
                if (active.Messages?.Count > 0)
                {
                    var vendor = GetCandidateVendorOrReturnSelf(active, Options.MobileOptions.AutoRetryAnotherVendor);
                    notificationResults.AddRange(vendor.SendAsync());

                }
            });




            return notificationResults;
        }

        private void SetActiveVendorToWorkingOne()
        {
            if (NotificationOptionsSingeltonWrapper.NotificationOptions?.EmailOptions?.AutoRetryAnotherVendor?.SetCandidateVendorAsDefault == true)
            {
                var workingVendor = Options.EmailOptions.ActiveVendors
                                                        .FirstOrDefault(x => x.IsFailed && x.MutatedCandidateVendor?.IsFailed == false)
                                                        ?.MutatedCandidateVendor
                                                        ?.OriginalMutatedCandidateVendor;
                if (workingVendor != null)
                {
                    NotificationOptionsSingeltonWrapper.NotificationOptions.EmailOptions.Vendors.ForEach(v => v.IsActive = false);
                    workingVendor.ResetRetryCount();
                    NotificationOptionsSingeltonWrapper.NotificationOptions
                                                       .EmailOptions
                                                       .SafeAddActiveVendorOrSetToActiveIfExist(workingVendor as EmailVendor);
                }
            }


            if (NotificationOptionsSingeltonWrapper.NotificationOptions?.SMSOptions?.AutoRetryAnotherVendor?.SetCandidateVendorAsDefault == true)
            {
                var workingVendor = Options.SMSOptions.ActiveVendors
                                                       .FirstOrDefault(x => x.IsFailed && x.MutatedCandidateVendor?.IsFailed == false)
                                                       ?.MutatedCandidateVendor
                                                       ?.OriginalMutatedCandidateVendor;
                if (workingVendor != null)
                {
                    NotificationOptionsSingeltonWrapper.NotificationOptions.SMSOptions.Vendors.ForEach(v => v.IsActive = false);
                    NotificationOptionsSingeltonWrapper.NotificationOptions
                                                       .SMSOptions
                                                       .SafeAddActiveVendorOrSetToActiveIfExist(workingVendor as SMSVendor);
                }
            }


            if (NotificationOptionsSingeltonWrapper.NotificationOptions?.MobileOptions?.AutoRetryAnotherVendor?.SetCandidateVendorAsDefault == true)
            {
                var workingVendor = Options.MobileOptions.ActiveVendors
                                                      .FirstOrDefault(x => x.IsFailed && x.MutatedCandidateVendor?.IsFailed == false)
                                                      ?.MutatedCandidateVendor
                                                      ?.OriginalMutatedCandidateVendor;
                if (workingVendor != null)
                {
                    NotificationOptionsSingeltonWrapper.NotificationOptions.MobileOptions.Vendors.ForEach(v => v.IsActive = false);
                    NotificationOptionsSingeltonWrapper.NotificationOptions
                                                       .MobileOptions
                                                       .SafeAddActiveVendorOrSetToActiveIfExist(workingVendor as MobilePushNotificationVendor);
                }
            }
        }
        private Vendor GetCandidateVendorOrReturnSelf<TMessage>(Vendor<TMessage> vendor, AutoRetryAnotherVendor autoRetryAnotherVendorParameter) where TMessage : MessagBase
        {
            var autoRetryAnotherVendor = autoRetryAnotherVendorParameter ?? new AutoRetryAnotherVendor();
            Type autoRetryExceptionType = null;

            var candidateVendor = GetWorkingVendorIfExist(vendor);
            if (!string.IsNullOrWhiteSpace(autoRetryAnotherVendor.ExceptionType))
            {
                autoRetryExceptionType = Utility.GetType(autoRetryAnotherVendor.ExceptionType);
            }

            if (vendor.Messages?.Count > 0)
            {
                var conditionalFaildMessage = vendor.Messages
                                        .FirstOrDefault(x => autoRetryAnotherVendor.Enable
                                                        && ((!string.IsNullOrWhiteSpace(x.LastFailedResult?.Exception?.Message)
                                                                && x.LastFailedResult
                                                                                .Exception
                                                                                .Message
                                                                                .Contains(autoRetryAnotherVendor.ExceptionMessageContains?.Trim() ?? string.Empty))
                                                            || (!string.IsNullOrWhiteSpace(x.LastFailedResult?.Exception.InnerException?.Message)
                                                                    && x.LastFailedResult
                                                                                    .Exception
                                                                                    .InnerException
                                                                                    .Message
                                                                                    .Contains(autoRetryAnotherVendor.ExceptionMessageContains?.Trim() ?? string.Empty)))
                                                        && (string.IsNullOrWhiteSpace(autoRetryAnotherVendor.ExceptionType)
                                                                || (x.LastFailedResult?.Exception != null
                                                                        && x.LastFailedResult.Exception.GetType() == autoRetryExceptionType)
                                                                || (x.LastFailedResult?.Exception.InnerException != null
                                                                        && x.LastFailedResult.Exception.InnerException.GetType() == autoRetryExceptionType)))
                                        ?.Results
                                        ?.LastOrDefault();

                if (conditionalFaildMessage != null && autoRetryAnotherVendor.CandidateVendorGlobalIndex == null)
                {
                    candidateVendor = Options.GetAllVendors(x => x.GlobalIndex != vendor.GlobalIndex
                                                             && !x.IsFailed
                                                             && x is Vendor<TMessage>)?.FirstOrDefault() as Vendor<TMessage>;

                    //candidateVendor = Options.GetAllVendors(x => x.GlobalIndex != vendor.GlobalIndex
                    //                                        && ((autoRetryAnotherVendor.AutoRetryFailedVendorsInCaseOfGlobalIndexNotSet && x.IsFailed) || !x.IsFailed)
                    //                                        && x is Vendor<TMessage>)?.FirstOrDefault() as Vendor<TMessage>;
                }
                else if (conditionalFaildMessage != null)
                {
                    candidateVendor = Options.GetAllVendors(x => x.GlobalIndex == autoRetryAnotherVendor.CandidateVendorGlobalIndex && x is Vendor<TMessage>)?.FirstOrDefault() as Vendor<TMessage>;

                }

                candidateVendor = candidateVendor ?? vendor;
                if (vendor.GlobalIndex != candidateVendor.GlobalIndex)
                {
                    var targetVendor = candidateVendor.DeepClone();
                    vendor.MutateTargetVendor(targetVendor, candidateVendor);
                    vendor.MutatedCandidateVendor = targetVendor;
                    candidateVendor = targetVendor;
                }

            }

            return candidateVendor;

        }

        private static Vendor<TMessage> GetWorkingVendorIfExist<TMessage>(Vendor<TMessage> vendor) where TMessage : MessagBase
        {
            HashSet<Vendor> visitedVendors = new HashSet<Vendor>();
            var candidateVendor = vendor;
            while (candidateVendor != null && candidateVendor.MutatedCandidateVendor != null && candidateVendor.IsFailed && !visitedVendors.Contains(candidateVendor))
            {
                visitedVendors.Add(candidateVendor);

                candidateVendor = candidateVendor.MutatedCandidateVendor as Vendor<TMessage>;
            }
            return candidateVendor;
        }

        public IFluentNotificationService WithEmail(EmailVendor emailVendor, EmailMessage message, int? retryCount = null)
        {
            var notification = this.DeepClone();
            emailVendor = emailVendor.DeepClone();
            emailVendor.RetryCount = retryCount ?? Options.EmailOptions.RetryCount;
            emailVendor.SafeAdd(message);
            notification?.Options?.EmailOptions?.SafeAddActiveVendor(emailVendor);
            return notification;
        }

        public IFluentNotificationService WithEmail(EmailMessage message)
        {
            var notification = this.DeepClone();
            notification?.Options?.EmailOptions?.ActiveVendors?.FirstOrDefault()?.SafeAdd(message);
            return notification;
        }


        public IFluentNotificationService WithSMS(SMSVendor smsVendor, SMSMessage message, int? retryCount = null)
        {
            var notification = this.DeepClone();
            smsVendor = smsVendor.DeepClone();
            smsVendor.RetryCount = retryCount ?? Options.SMSOptions.RetryCount;
            smsVendor.SafeAdd(message);
            notification?.Options?.SMSOptions?.SafeAddActiveVendor(smsVendor);
            return notification;
        }

        public IFluentNotificationService WithSMS(SMSMessage message)
        {
            var notification = this.DeepClone();
            notification?.Options?.SMSOptions?.ActiveVendors?.FirstOrDefault()?.SafeAdd(message);
            return notification;
        }

        public IFluentNotificationService WithMobilePushNotification(MobilePushNotificationVendor mobileVendor, MobileNotificationMessage message, int? retryCount = null)
        {
            var notification = this.DeepClone();
            mobileVendor = mobileVendor.DeepClone();
            mobileVendor.RetryCount = retryCount ?? Options.MobileOptions.RetryCount;
            mobileVendor.SafeAdd(message);
            notification?.Options?.MobileOptions?.SafeAddActiveVendor(mobileVendor);
            return notification;
        }

        public IFluentNotificationService WithMobilePushNotification(MobileNotificationMessage message)
        {
            var notification = this.DeepClone();
            notification?.Options?.MobileOptions?.ActiveVendors?.FirstOrDefault()?.SafeAdd(message);
            return notification;
        }

        //public INotificationService WithWeb(EmailVendor emailVendor, EmailMessage message)
        //{
        //    throw new NotImplementedException();
        //}

        //public INotificationService WithWeb(EmailMessage message)
        //{
        //    throw new NotImplementedException();
        //}


    }
}
