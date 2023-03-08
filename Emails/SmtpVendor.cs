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
        internal override Task<NotificationResult>[] SendAsync()
        {


            var notificationResults = new Task<NotificationResult>[Messages.Count];
            for (int i = 0; i < Messages.Count; i++)
            {
                var message = Messages[i];
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

                foreach (var cc in message.CC ?? Enumerable.Empty<string>())
                    mailMessage.CC.Add(cc);

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
                                                       if (a is MemoryAttachment memoryAttachment)
                                                           memoryAttachment.ResetContent();
                                                   });
                                                   var vendor = new SmtpVendor(Server, Port, UserName, "***", UseSSL, message);
                                                   message = null;

                                                   return r.IsCompletedSuccessfully ? NotificationResult.Success(vendor, requestOn) :
                                                                                      NotificationResult.Fail(vendor, requestOn, r.Exception);
                                               });

            }

            return notificationResults.ToArray();

        }
    }
}
