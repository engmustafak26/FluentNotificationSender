using FirebaseAdmin.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FluentNotificationSender.Abstractions;
using SendGrid;
using System.Numerics;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json;
using EllipticCurve;

namespace FluentNotificationSender.MobileNotifications
{
    public sealed class FirebaseVendor : MobilePushNotificationVendor
    {
        public FirebaseVendor()
        {

        }
        public FirebaseVendor(string jsonConfiguration)
        {
            var vendorObj = JsonConvert.DeserializeObject<FirebaseVendor>(jsonConfiguration);
            this.Type = vendorObj.Type;
            this.Project_Id = vendorObj.Project_Id;
            this.Private_Key_Id = vendorObj.Private_Key_Id;
            this.Private_Key = vendorObj.Private_Key;
            this.Client_Email = vendorObj.Client_Email;
            this.Client_Id = vendorObj.Client_Id;
            this.Auth_Uri = vendorObj.Auth_Uri;
            this.Token_Uri = vendorObj.Token_Uri;
            this.Auth_Provider_X509_Cert_Url = vendorObj.Auth_Provider_X509_Cert_Url;
            this.Client_X509_Cert_Url = vendorObj.Client_X509_Cert_Url;
            vendorObj = null;
        }

        public FirebaseVendor(string type, string project_Id, string private_Key_Id, string private_Key, string client_Email, string client_Id, string auth_Uri, string token_Uri, string auth_Provider_X509_Cert_Url, string client_X509_Cert_Url)
        {
            Type = type;
            Project_Id = project_Id;
            Private_Key_Id = private_Key_Id;
            Private_Key = private_Key;
            Client_Email = client_Email;
            Client_Id = client_Id;
            Auth_Uri = auth_Uri;
            Token_Uri = token_Uri;
            Auth_Provider_X509_Cert_Url = auth_Provider_X509_Cert_Url;
            Client_X509_Cert_Url = client_X509_Cert_Url;
        }
        internal FirebaseVendor(string type, string project_Id, string private_Key_Id, string private_Key, string client_Email,
                                 string client_Id, string auth_Uri, string token_Uri, string auth_Provider_X509_Cert_Url,
                                 string client_X509_Cert_Url, MobileNotificationMessage message) : this(type, project_Id, private_Key_Id, private_Key, client_Email,
                                                                                        client_Id, auth_Uri, token_Uri, auth_Provider_X509_Cert_Url, client_X509_Cert_Url)
        {
            SafeAdd(message);
        }


        public string Type { get; set; }
        public string Project_Id { get; set; }
        public string Private_Key_Id { get; set; }
        public string Private_Key { get; set; }
        public string Client_Email { get; set; }
        public string Client_Id { get; set; }
        public string Auth_Uri { get; set; }
        public string Token_Uri { get; set; }
        public string Auth_Provider_X509_Cert_Url { get; set; }
        public string Client_X509_Cert_Url { get; set; }


        internal override void SupressSensitiveInfo()
        {
            this.Private_Key_Id = SupressString;
            this.Private_Key = SupressString;
        }

        internal override Task<FluentNotificationResult>[] SendAsync()
        {

            var app = FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromJson(JsonConvert.SerializeObject(this, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                })),
            }, name: Guid.NewGuid().ToString());
            var requestOn = DateTime.Now;

            var canRetryMessages = Messages.Where(x => x.CanRetry).ToArray();
            var firbaseMessages = canRetryMessages?.Select(x => new Message
            {
                Token = x.Token,
                Topic = x.Topic,
                Android = x.Android,
                Apns = x.Apns,
                Condition = x.Condition,
                Data = x.Data,
                FcmOptions = x.FcmOptions,
                Notification = x.Notification,
                Webpush = x.Webpush
            });
            if (!firbaseMessages.Any())
            {
                return new Task<FluentNotificationResult>[0];
            }
            return FirebaseMessaging.GetMessaging(app).SendAllAsync(firbaseMessages)
                                               .ContinueWith(n =>
                                               {
                                                   app.Delete();
                                                   app = null;
                                                   var notificationResults = new Task<FluentNotificationResult>[canRetryMessages.Length];
                                                   int index = 0;
                                                   foreach (var response in n.Result.Responses)
                                                   {
                                                       var baseResult = GetNotificationResponse(n, response);
                                                       canRetryMessages[index].SafeAddResult(baseResult, this);

                                                       var vendor = new FirebaseVendor(this.Type, this.Project_Id, Private_Key_Id, Private_Key, this.Client_Email, "***", this.Auth_Uri,
                                                                                     this.Token_Uri, this.Auth_Provider_X509_Cert_Url, this.Client_X509_Cert_Url, canRetryMessages[index]);
                                                       vendor.SupressSensitiveInfo();

                                                       notificationResults[index] = Task.FromResult(FluentNotificationResult
                                                                                                            .FromNotificationResult(baseResult, vendor));
                                                       index++;
                                                   }
                                                   return notificationResults;

                                               }).Result;

            FluentNotificationResult GetNotificationResponse(Task<BatchResponse> responseTask, SendResponse response)
            {
                if (!responseTask.IsCompletedSuccessfully)
                    return FluentNotificationResult.Fail(this, requestOn, response.Exception);
                return response.IsSuccess ? FluentNotificationResult.Success(this, requestOn) : FluentNotificationResult.Fail(this, requestOn, response.Exception);

            }


        }
    }
}
