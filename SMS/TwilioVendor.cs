using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentNotificationSender.Abstractions;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace FluentNotificationSender.SMS
{
    public class TwilioVendor : SMSVendor
    {
        public TwilioVendor()
        {

        }
        public TwilioVendor(string accountSid, string authToken, string fromNumber)
        {

            AccountSid = accountSid;
            AuthToken = authToken;
            FromNumber = fromNumber;
        }

        internal override void SupressSensitiveInfo()
        {
            this.AccountSid = SupressString;
            this.AuthToken = SupressString;
        }

        internal TwilioVendor(string accountSid, string authToken, string fromNumber, SMSMessage message) : this(accountSid, authToken, fromNumber)
        {
            SafeAdd(message);
        }
        public string AccountSid { get; set; }
        public string AuthToken { get; set; }
        public string FromNumber { get; set; }

        internal override Task<FluentNotificationResult>[] SendAsync()
        {
            var canRetryMessages = Messages.Where(x => x.CanRetry).ToArray();
            var notificationResults = new Task<FluentNotificationResult>[canRetryMessages.Length];

            for (int i = 0; i < canRetryMessages.Length; i++)
            {
                var message = canRetryMessages[i];
                TwilioClient.Init(AccountSid, AuthToken);
                var requestOn = DateTime.Now;
                notificationResults[i] = MessageResource.CreateAsync(
                                             body: message.Message,
                                             from: new Twilio.Types.PhoneNumber(FromNumber),
                                             to: new Twilio.Types.PhoneNumber(message.MobileNumber)).ContinueWith(async r =>
                {


                    TwilioVendor vendor = null;
                    FluentNotificationResult baseResult = null;
                    if (r.IsCompleted && r.Status == TaskStatus.RanToCompletion && r.Result?.Status != MessageResource.StatusEnum.Failed)
                    {
                        string response = $"Sid:{(await r).Sid}, Price:{(await r).Price}, Price Unit:{(await r).PriceUnit}";
                        baseResult = FluentNotificationResult.Success(this, requestOn, response);
                        message.SafeAddResult(baseResult, this);
                        vendor = new TwilioVendor(AccountSid, AuthToken, FromNumber, message);
                        vendor.SupressSensitiveInfo();

                        return FluentNotificationResult.FromNotificationResult(baseResult, vendor);
                    }
                    baseResult = FluentNotificationResult.Fail(this, requestOn, r.Exception);
                    message.SafeAddResult(baseResult, this);
                    vendor = new TwilioVendor(AccountSid, AuthToken, FromNumber, message);
                    vendor.SupressSensitiveInfo();

                    return FluentNotificationResult.FromNotificationResult(baseResult, vendor);

                }).ContinueWith(r => r.Result?.Result);
            }

            return notificationResults;
        }
    }
}
