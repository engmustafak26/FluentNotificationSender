using FluentNotificationSender.Abstractions;
using FluentNotificationSender.Extensions;
using FluentNotificationSender.Helpers;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FluentNotificationSender.Emails
{
    public sealed class SendGridVendor : EmailVendor
    {
        private const string SendGridEmptyTemplate = " ";
        public SendGridVendor()
        {

        }
        public SendGridVendor(string fromEmail, string fromName, string apiKey)
        {
            FromEmail = fromEmail;
            FromName = fromName;
            ApiKey = apiKey;
        }
        internal SendGridVendor(string fromEmail, string fromName, string apiKey, EmailMessage message) : this(fromEmail, fromName, apiKey)
        {
            SafeAdd(message);
        }

        public string FromEmail { get; set; }
        public string FromName { get; set; }
        public string ApiKey { get; set; }

        internal override void SupressSensitiveInfo()
        {
            this.ApiKey = SupressString;
        }
        internal override Task<FluentNotificationResult>[] SendAsync()
        {

            var canRetryMessages = Messages.Where(x => x.CanRetry).ToArray();
            var notificationResults = new Task<FluentNotificationResult>[canRetryMessages.Length];
            for (int i = 0; i < canRetryMessages.Length; i++)
            {
                var message = canRetryMessages[i];

                var client = new SendGridClient(ApiKey);
                var mailMessage = new SendGridMessage()
                {
                    From = new EmailAddress(FromEmail, FromName),
                    Subject = message.Subject,
                    PlainTextContent = message.Body ?? SendGridEmptyTemplate
                };
                if (message.IsHtmlBody)
                {
                    mailMessage.HtmlContent = message.Body ?? SendGridEmptyTemplate;
                }


                mailMessage.AddTos(message.To.Select(t => new EmailAddress(t)).ToList());

                if (message.CC != null && message.CC.Any())
                {
                    mailMessage.AddCcs(message.CC?.Select(c => new EmailAddress(c)).ToList());
                }
                if (message.Attachments != null)
                {
                    foreach (var attachment in message.Attachments)
                    {
                        if (attachment is FilePathAttachment filePathAttach)
                        {
                            mailMessage.AddAttachment(Path.GetFileName(filePathAttach.FileName), Utility.ConvertFileToBase64(filePathAttach.FileName), filePathAttach.MediaType);
                        }
                        else if (attachment is MemoryAttachment memoryAttach)
                        {
                            mailMessage.AddAttachment(memoryAttach.Name, memoryAttach.Content.ToBase64(), memoryAttach.MediaType);
                        }

                    }
                }

                var requestOn = DateTime.Now;
                notificationResults[i] = client.SendEmailAsync(mailMessage)
                                               .ContinueWith(r =>
                                               {
                                                   client = null;
                                                   message.Attachments?.ForEach(a =>
                                                   {
                                                       //if (a is MemoryAttachment memoryAttachment)
                                                       //    memoryAttachment.ResetContent();
                                                   });

                                                   var baseResult = r.IsCompleted && r.Status == TaskStatus.RanToCompletion ? (r.Result.IsSuccessStatusCode ? FluentNotificationResult.Success(this, requestOn) :
                                                                                      FluentNotificationResult.Fail(this, requestOn, new Exception(r.Result.Body.ReadAsStringAsync().Result))) :
                                                                                      FluentNotificationResult.Fail(this, requestOn, r.Exception);


                                                   message.SafeAddResult(baseResult, this);

                                                   var vendor = new SendGridVendor(FromEmail, FromName, ApiKey, message);
                                                   vendor.SupressSensitiveInfo();


                                                   return FluentNotificationResult
                                                                                .FromNotificationResult(baseResult, vendor);
                                               });

            }

            return notificationResults.ToArray();

        }


    }
}

