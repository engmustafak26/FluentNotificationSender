using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNotificationSender.Abstractions;
using Newtonsoft.Json;

namespace FluentNotificationSender
{
    public class FluentNotificationResult
    {
        public bool IsSuccess { get; internal set; }
        public DateTime RequestedOn { get; private set; }
        public DateTime ResponseOn { get; internal set; }
        public TimeSpan ElapsedTime
        {
            get
            {
                return ResponseOn.Subtract(RequestedOn);
            }
            set
            {

            }
        }
        public string VendorType { get; private set; }
        public Vendor VendorInfo { get; private set; }
        public string ResponseMessage { get; internal set; }

        public Exception Exception { get; internal set; }


        private FluentNotificationResult(Vendor vendor, DateTime requestOn, string responseMessage = null)
        {
            IsSuccess = true;
            VendorInfo = vendor;
            VendorType = VendorInfo.GetType().Name;
            RequestedOn = requestOn;
            ResponseOn = DateTime.Now;
            ResponseMessage = responseMessage;
        }
        private FluentNotificationResult(Vendor vendor, DateTime requestOn, Exception exception)
        {
            IsSuccess = false;
            VendorInfo = vendor;
            VendorType = VendorInfo.GetType().Name;
            RequestedOn = requestOn;
            ResponseOn = DateTime.Now;
            Exception = exception;
        }
        [JsonConstructor]
        private FluentNotificationResult(Vendor vendor, bool isSuccess, DateTime requestOn, Exception exception, string responseMessage = null)
        {
            IsSuccess = isSuccess;
            VendorInfo = vendor;
            VendorType = VendorInfo?.GetType().Name;
            RequestedOn = requestOn;
            ResponseOn = DateTime.Now;
            Exception = exception;
            ResponseMessage = responseMessage;
        }
        public static FluentNotificationResult Success(Vendor vendor, DateTime requestOn, string responseMessage = null)
        {
            return new FluentNotificationResult(vendor, requestOn, responseMessage);
        }
        public static FluentNotificationResult Fail(Vendor vendor, DateTime requestOn, Exception exception)
        {
            return new FluentNotificationResult(vendor, requestOn, exception);
        }

        internal static FluentNotificationResult FromNotificationResult(FluentNotificationResult result, Vendor vendor)
        {
            return new FluentNotificationResult(vendor, result.IsSuccess, result.RequestedOn, result.Exception, result.ResponseMessage);
        }
    }
}
