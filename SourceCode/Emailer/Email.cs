using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;

namespace Emailer
{
    public class Email
    {
        private bool _disposed;
        public string From { get; set; }
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public MailMessage MailMessage { get; set; }
        public List<Attachment> Attachments { get; set; }

        public Email(string from, List<string> toList, string subject, string body)
        {
            From = from;
            Subject = subject;
            Body = body;
            To = GetEmailToList(toList);
        }

        public Email(string from, string commaDelimitedToList, string subject, string body)
        {
            From = from;
            Subject = subject;
            Body = body;
            To = commaDelimitedToList;
        }

        public Email(string from, List<string> toList, string subject, string body, List<Attachment> filesToAttach)
        {
            From = from;
            Subject = subject;
            Body = body;
            To = GetEmailToList(toList);
            Attachments = filesToAttach;
        }

        public Email(string from, string commaDelimitedToList, string subject, string body, List<Attachment> filesToAttach)
        {
            From = from;
            Subject = subject;
            Body = body;
            To = commaDelimitedToList;
            Attachments = filesToAttach;
        }

        public Response Send(IEmailService emailService)
        {
            Response sendResponse = new Response(true, string.Empty);

            try
            {
                MailMessage = new MailMessage
                {
                    From = new MailAddress(From),
                    Subject = Subject,
                    Body = Body,
                    IsBodyHtml = true
                };

                if (Attachments != null && Attachments.Any())
                {
                    foreach (Attachment file in Attachments)
                    {
                        MailMessage.Attachments.Add(file);
                    }
                }

                MailMessage.To.Add(To);
                sendResponse = emailService.Send(MailMessage);
            }
            catch (Exception ex)
            {
                sendResponse.Success = false;
                sendResponse.Message = ex.ToString();
            }

            return sendResponse;
        }

        private static string GetEmailToList(List<string> toList)
        {
            return string.Join(",", toList);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (MailMessage != null)
                {
                    MailMessage.Dispose();
                }

                if (Attachments != null && Attachments.Any())
                {
                    foreach (Attachment file in Attachments)
                    {
                        file.Dispose();
                    }
                }
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
