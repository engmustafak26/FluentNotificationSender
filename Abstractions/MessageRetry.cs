using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNotificationSender.Emails;
using FluentNotificationSender.Interfaces;

namespace FluentNotificationSender.Abstractions
{
    public abstract class MessageRetry
    {
        private protected MessageRetry()
        {
        }

        [JsonProperty]
        internal List<FluentNotificationResult> Results { get; set; } = null;
        [JsonProperty]
        internal int RetryCount { get; set; }
        [JsonProperty]
        internal int RunningCount { get; set; }
        [JsonIgnore]
        internal FluentNotificationResult LastFailedResult => Results?.LastOrDefault()?.IsSuccess == false ? Results.LastOrDefault() : null;
        [JsonIgnore]
        internal bool CanRetry => (Results == null || Results.Count == 0) || (!Results.LastOrDefault().IsSuccess && RunningCount < RetryCount);

        internal void SafeAddResult(FluentNotificationResult result, Vendor ownVendor)
        {
            ownVendor.IsFailed = !result.IsSuccess;
            Results = Results ?? new List<FluentNotificationResult>();
            Results?.Add(result);
            RunningCount++;
        }

        internal void SetRetryCont(int retryCount)
        {
            this.RetryCount = retryCount;
        }

    }
}
