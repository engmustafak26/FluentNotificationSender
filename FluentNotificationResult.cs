using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNotificationSender.Abstractions;

namespace FluentNotificationSender
{
    public class FluentNotificationResult
    {
        public bool IsSuccess { get; private set; }
        public DateTime RequestedOn { get; private set; }
        public DateTime ResponseOn { get; private set; }
        public TimeSpan ElapsedTime => ResponseOn.Subtract(RequestedOn);
        public string VendorType { get; private set; }
        public Vendor VendorInfo { get; private set; }
        public string ResponseMessage { get; private set; }
   
  
        public Exception Exception { get; private set; }

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
        public static FluentNotificationResult Success(Vendor vendor, DateTime requestOn, string responseMessage = null)
        {
            return new FluentNotificationResult(vendor, requestOn, responseMessage);
        }
        public static FluentNotificationResult Fail(Vendor vendor, DateTime requestOn, Exception exception)
        {
            return new FluentNotificationResult(vendor, requestOn, exception);
        }
    }
}
