using Application.Interfaces;
using Application.Models;

namespace Infrastructure.Email
{
    public class EmailDispatcher : IEmailDispatcher
    {
        private readonly EmailQueue _queue;

        public EmailDispatcher(EmailQueue queue)
        {
            _queue = queue;
        }

        public Task EnqueueAsync(EmailMessage message)
        {
            _queue.Enqueue(message);
            return Task.CompletedTask;
        }
    }
}