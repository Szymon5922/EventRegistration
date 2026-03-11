using Application.Models;

namespace Infrastructure.Email
{
    public interface IEmailSender
    {
        Task<bool> SendSingleAsync(EmailMessage message);
        Task<bool> SendBatchAsync(IEnumerable<EmailMessage> messages);
    }
}
