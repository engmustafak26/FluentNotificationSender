using System;
using System.Collections.Generic;
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

        internal UnifonicVendor(string aPIUrl, string appSid, string senderID, SMSMessage message) : this(aPIUrl, appSid, senderID)
        {
            SafeAdd(message);
        }
        public string APIUrl { get; set; }
        public string AppSid { get; set; }
        public string SenderID { get; set; }

        internal override Task<FluentNotificationResult>[] SendAsync()
        {
            var notificationResults = new Task<FluentNotificationResult>[Messages.Count];

            for (int i = 0; i < Messages.Count; i++)
            {
                var message = Messages[i];
                HttpClient client = new HttpClient();
                string fullUrl = $"{APIUrl}?AppSid={AppSid}&SenderID={SenderID}&Body={message.Message}&Recipient={message.MobileNumber}";
                var requestOn = DateTime.Now;
                notificationResults[i] = client.PostAsync(fullUrl, new StringContent(string.Empty)).ContinueWith(async r =>
                {
                    client?.Dispose();
                    client = null;

                    var vendor = new UnifonicVendor(APIUrl, AppSid, SenderID, message);
                    message = null;

                    if (r.IsCompletedSuccessfully)
                    {
                        string response = await (await r).Content.ReadAsStringAsync();
                        return FluentNotificationResult.Success(vendor, requestOn, response);
                    }
                    return FluentNotificationResult.Fail(vendor, requestOn, r.Exception);

                }).ContinueWith(r => r.Result.Result);
            }

            return notificationResults;
        }
    }
}
