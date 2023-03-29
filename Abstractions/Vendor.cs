using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNotificationSender.Interfaces;
using FluentNotificationSender.Extensions;

namespace FluentNotificationSender.Abstractions
{
    public abstract class Vendor : INotificationMethod
    {
        internal const string SupressString = "***";
        private protected Vendor()
        {
        }

        [JsonProperty]
        public int GlobalIndex { get; set; }
        [JsonIgnore]
        internal bool UseAsDefault { get; set; }
        internal Vendor MutatedCandidateVendor { get; set; }
        internal Vendor OriginalMutatedCandidateVendor { get; set; }
        private bool _isFailed { get; set; }
        internal bool IsFailed
        {
            get { return _isFailed; }
            set
            {
                _isFailed = value;
                if (OriginalMutatedCandidateVendor != null)
                {
                    OriginalMutatedCandidateVendor.IsFailed = value;
                }
            }
        }
        internal bool IsActive { get; set; }
        internal int? RetryCount { get; set; }
        internal abstract Task<FluentNotificationResult>[] SendAsync();
        internal abstract void SupressSensitiveInfo();
        internal abstract void MutateTargetVendor<MMessage>(Vendor<MMessage> targetVendor, Vendor originalRefrence) where MMessage : MessagBase;
        internal abstract void ClearMessages();
        internal void ResetRetryCount()
        {
            RetryCount = 0;
        }
    }
    public abstract class Vendor<TMessage> : Vendor, INotificationMethod where TMessage : MessagBase
    {
        private protected Vendor()
        {
        }



        [JsonProperty]
        internal List<TMessage> Messages { get; set; }
        internal void SafeAdd(TMessage message)
        {
            if (this.Messages?.FirstOrDefault(x => x.Id == message.Id) != null)
                return;

            if (string.IsNullOrWhiteSpace(message.Id))
                message.Id = Guid.NewGuid().ToString();

            message.SetRetryCont(message.RetryCount == 0 ? this.RetryCount.GetValueOrDefault() : message.RetryCount);
            Messages = Messages ?? new List<TMessage>();
            Messages.Add(message);
        }
        internal override void MutateTargetVendor<MMessage>(Vendor<MMessage> targetVendor, Vendor originalRefrence)
        {
            targetVendor.RetryCount = this.RetryCount;
            targetVendor.Messages = this.Messages as List<MMessage>;
            targetVendor.OriginalMutatedCandidateVendor = originalRefrence;

        }
        internal override void ClearMessages()
        {
            Messages.ForEach(x => x = null);
            Messages.Clear();
        }
        internal Vendor<MessagBase> ToBase()
        {
            return this as Vendor<MessagBase>;
        }


    }
}
