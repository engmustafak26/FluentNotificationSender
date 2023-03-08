﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNotificationSender.Abstractions;

namespace FluentNotificationSender
{
    public class NotificationResult
    {
        public bool IsSuccess { get; private set; }
        public DateTime RequestedOn { get; private set; }
        public DateTime ResponseOn { get; private set; }
        public TimeSpan ElapsedTime => ResponseOn.Subtract(RequestedOn);
        public string ResponseMessage { get; private set; }
        public Vendor VendorInfo { get; private set; }
        public Exception Exception { get; private set; }

        private NotificationResult(Vendor vendor, DateTime requestOn, string responseMessage = null)
        {
            IsSuccess = true;
            VendorInfo = vendor;
            RequestedOn = requestOn;
            ResponseOn = DateTime.Now;
            ResponseMessage = responseMessage;
        }
        private NotificationResult(Vendor vendor, DateTime requestOn, Exception exception)
        {
            IsSuccess = false;
            VendorInfo = vendor;
            RequestedOn = requestOn;
            ResponseOn = DateTime.Now;
            Exception = exception;
        }
        public static NotificationResult Success(Vendor vendor, DateTime requestOn, string responseMessage = null)
        {
            return new NotificationResult(vendor, requestOn, responseMessage);
        }
        public static NotificationResult Fail(Vendor vendor, DateTime requestOn, Exception exception)
        {
            return new NotificationResult(vendor, requestOn, exception);
        }
    }
}
