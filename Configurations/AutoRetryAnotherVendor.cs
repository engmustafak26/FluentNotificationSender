using System;
using System.Collections.Generic;
using System.Text;

namespace FluentNotificationSender.Configurations
{
    public sealed class AutoRetryAnotherVendor
    {
        public bool Enable { get; set; }
        public string ExceptionType { get; set; }
        public string ExceptionMessageContains { get; set; }
        public int? CandidateVendorGlobalIndex { get; set; }
        //public bool AutoRetryFailedVendorsInCaseOfGlobalIndexNotSet { get; set; }
        public bool SetCandidateVendorAsDefault { get; set; }
    }
}
