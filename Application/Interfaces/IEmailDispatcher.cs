using Application.Models;

namespace Application.Interfaces
{
    public interface IEmailDispatcher
    {
        Task EnqueueAsync(EmailMessage message);
    }
}
