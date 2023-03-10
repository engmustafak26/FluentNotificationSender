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
        private const string SendGridEmptyTemplate= " ";
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
        internal override Task<FluentNotificationResult>[] SendAsync()
        {


            var notificationResults = new Task<FluentNotificationResult>[Messages.Count];
            for (int i = 0; i < Messages.Count; i++)
            {
                var message = Messages[i];

                var client = new SendGridClient(ApiKey);
                var mailMessage = new SendGridMessage()
                {
                    From = new EmailAddress(FromEmail, FromName),
                    Subject = message.Subject,
                    PlainTextContent = message.Body?? SendGridEmptyTemplate
                };
                if (message.IsHtmlBody)
                {
                    mailMessage.HtmlContent = message.Body?? SendGridEmptyTemplate;
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
                                                       if (a is MemoryAttachment memoryAttachment)
                                                           memoryAttachment.ResetContent();
                                                   });
                                                   var vendor = new SendGridVendor(FromEmail, FromName, "***", message);
                                                   message = null;

                                                   return r.IsCompletedSuccessfully ? (r.Result.IsSuccessStatusCode ? FluentNotificationResult.Success(vendor, requestOn) :
                                                                                      FluentNotificationResult.Fail(vendor, requestOn, new Exception(r.Result.Body.ReadAsStringAsync().Result))) :
                                                                                      FluentNotificationResult.Fail(vendor, requestOn, r.Exception);
                                               });

            }

            return notificationResults.ToArray();

        }
    }
}

