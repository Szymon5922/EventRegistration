using Contracts;

namespace Infrastructure.Email
{
    public interface IEmailSender
    {
        Task<bool> SendSingleAsync(IEmailRequest message);
        Task<bool> SendBatchAsync(IEnumerable<IEmailRequest> messages);
    }
}
