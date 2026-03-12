using MailFunctions.Models;

namespace MailFunctions.Interfaces
{
    public interface IEmailSender
    {
        Task<bool> SendSingleAsync(EmailMessage email);
    }
}
