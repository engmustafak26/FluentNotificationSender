using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using FluentNotificationSender.Abstractions;
using FluentNotificationSender.Extensions;

namespace FluentNotificationSender.Emails
{
    public sealed class SmtpVendor : EmailVendor
    {
        public SmtpVendor()
        {

        }
        public SmtpVendor(string server, int port, string userName, string password, bool useSSL)
        {
            Server = server;
            Port = port;
            UserName = userName;
            Password = password;
            UseSSL = useSSL;
        }
        internal SmtpVendor(string server, int port, string userName, string password, bool useSSL, EmailMessage message) : this(server, port, userName, password, useSSL)
        {
            SafeAdd(message);
        }

        public string Server { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool UseSSL { get; set; }

        internal override void SupressSensitiveInfo()
        {
            this.Password = SupressString;
        }
        internal override Task<FluentNotificationResult>[] SendAsync()
        {

            var canRetryMessages = Messages.Where(x => x.CanRetry).ToArray();
            var notificationResults = new Task<FluentNotificationResult>[canRetryMessages.Length];
            for (int i = 0; i < canRetryMessages.Length; i++)
            {
                var message = canRetryMessages[i];
                SmtpClient client = new SmtpClient(Server, Port) { EnableSsl = UseSSL };
                client.Credentials = new NetworkCredential(UserName, Password);
                client.EnableSsl = UseSSL;
                MailMessage mailMessage = new MailMessage();

                mailMessage.From = new MailAddress(UserName);
                mailMessage.BodyEncoding = Encoding.UTF8;

                mailMessage.Subject = message.Subject;
                mailMessage.Body = message.Body;
                mailMessage.IsBodyHtml = message.IsHtmlBody;

                foreach (var to in message.To)
                    mailMessage.To.Add(to);

                if (message.CC != null && message.CC.Any())
                {
                    foreach (var cc in message.CC)
                        mailMessage.CC.Add(cc);
                }

                if (message.Attachments != null)
                {
                    foreach (var attachment in message.Attachments)
                    {
                        if (attachment is FilePathAttachment filePathAttach)
                            mailMessage.Attachments.Add(new Attachment(filePathAttach.FileName, filePathAttach.MediaType));

                        else if (attachment is MemoryAttachment memoryAttach)
                            mailMessage.Attachments.Add(new Attachment(memoryAttach.Content.ToStream(), memoryAttach.Name, memoryAttach.MediaType));
                    }
                }

                var requestOn = DateTime.Now;
                notificationResults[i] = client.SendMailAsync(mailMessage)
                                               .ContinueWith(r =>
                                               {
                                                   client?.Dispose();
                                                   client = null;
                                                   message.Attachments?.ForEach(a =>
                                                   {
                                                       //if (a is MemoryAttachment memoryAttachment) 
                                                       //memoryAttachment.ResetContent();
                                                   });

                                                   var baseResult = r.IsCompleted && r.Status == TaskStatus.RanToCompletion ? FluentNotificationResult.Success(this, requestOn) :
                                                                                     FluentNotificationResult.Fail(this, requestOn, r.Exception);

                                                   message.SafeAddResult(baseResult, this);

                                                   var vendor = new SmtpVendor(Server, Port, UserName, Password, UseSSL, message);
                                                   vendor.SupressSensitiveInfo();

                                                   return FluentNotificationResult
                                                                           .FromNotificationResult(baseResult, vendor);
                                               });

            }

            return notificationResults.ToArray();

        }
    }
}
