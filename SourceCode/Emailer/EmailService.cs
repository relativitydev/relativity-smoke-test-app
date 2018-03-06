using System;
using System.Net;
using System.Net.Mail;

namespace Emailer
{
    public class EmailService : IEmailService
    {
        public SmtpClient Client { get; set; }
        public Boolean SmtpRequired { get; set; }

        public EmailService(string smtpServer, string smtpUsername, string smtpPassword, Int32? smtpPort)
        {
            Client = new SmtpClient(smtpServer);
            if (!string.IsNullOrEmpty(smtpUsername) && !string.IsNullOrEmpty(smtpPassword))
            {
                Client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
            }
            if (smtpPort != null)
            {
                Client.Port = smtpPort.Value;
            }
            Client.EnableSsl = SmtpRequired;
        }

        public Response Send(MailMessage mailMessage)
        {
            Response response = new Response(true, string.Empty);

            try
            {
                Client.Send(mailMessage);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.ToString();
            }

            return response;
        }
    }
}
