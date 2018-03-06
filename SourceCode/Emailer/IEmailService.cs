using System.Net.Mail;

namespace Emailer
{
    public interface IEmailService
    {
        Response Send(MailMessage mailMessage);
    }
}
