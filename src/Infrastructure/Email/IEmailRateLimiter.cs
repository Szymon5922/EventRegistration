using Application.Models;

namespace Infrastructure.Email
{
    public interface IEmailRateLimiter
    {
        public Task ProcessAsync(EmailMessage message, CancellationToken token);
    }
}
