using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentNotificationSender.Abstractions;

namespace FluentNotificationSender.SMS
{
    public class UnifonicVendor : SMSVendor
    {
        public UnifonicVendor()
        {

        }
        public UnifonicVendor(string aPIUrl, string appSid, string senderID)
        {
            APIUrl = aPIUrl;
            AppSid = appSid;
            SenderID = senderID;
        }

        internal override void SupressSensitiveInfo()
        {
            this.AppSid = SupressString;
            this.SenderID = SupressString;
        }

        internal UnifonicVendor(string aPIUrl, string appSid, string senderID, SMSMessage message) : this(aPIUrl, appSid, senderID)
        {
            SafeAdd(message);
        }
        public string APIUrl { get; set; }
        public string AppSid { get; set; }
        public string SenderID { get; set; }

        internal override Task<FluentNotificationResult>[] SendAsync()
        {
            var canRetryMessages = Messages.Where(x => x.CanRetry).ToArray();
            var notificationResults = new Task<FluentNotificationResult>[canRetryMessages.Length];

            for (int i = 0; i < canRetryMessages.Length; i++)
            {
                var message = canRetryMessages[i];
                HttpClient client = new HttpClient();
                string fullUrl = $"{APIUrl}?AppSid={AppSid}&SenderID={SenderID}&Body={message.Message}&Recipient={message.MobileNumber}";
                var requestOn = DateTime.Now;
                notificationResults[i] = client.PostAsync(fullUrl, new StringContent(string.Empty)).ContinueWith(async r =>
                {
                    client?.Dispose();
                    client = null;

                    UnifonicVendor vendor = null;
                    FluentNotificationResult baseResult = null;
                    if (r.IsCompleted && r.Result.IsSuccessStatusCode)
                    {
                        string response = await (await r).Content.ReadAsStringAsync();
                        baseResult = FluentNotificationResult.Success(this, requestOn, response);
                        message.SafeAddResult(baseResult, this);
                        vendor = new UnifonicVendor(APIUrl, AppSid, SenderID, message);
                        vendor.SupressSensitiveInfo();

                        return FluentNotificationResult.FromNotificationResult(baseResult, vendor);
                    }
                    baseResult = FluentNotificationResult.Fail(this, requestOn, r.Exception);
                    message.SafeAddResult(baseResult, this);
                    vendor = new UnifonicVendor(APIUrl, AppSid, SenderID, message);
                    vendor.SupressSensitiveInfo();

                    return FluentNotificationResult.FromNotificationResult(baseResult, vendor);

                }).ContinueWith(r => r.Result.Result);
            }

            return notificationResults;
        }
    }
}
