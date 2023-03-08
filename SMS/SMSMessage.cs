using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace FluentNotificationSender.SMS
{
    public class SMSMessage
    {
        [JsonConstructor]
        public SMSMessage(string mobileNumber, string message)
        {
            MobileNumber = mobileNumber;
            Message = message;
        }

        public string MobileNumber { get; private set; }
        public string Message { get; private set; }
    }


}
