using Application.Interfaces;
using Contracts;

namespace Infrastructure.Email
{
    public class InMemoryEmailDispatcher : IEmailDispatcher
    {
        private readonly InMemoryEmailQueue _queue;

        public InMemoryEmailDispatcher(InMemoryEmailQueue queue)
        {
            _queue = queue;
        }

        public Task EnqueueAsync(RegistrationCompletedEmailRequested request)
        {
            _queue.Enqueue(request);
            return Task.CompletedTask;
        }

        public Task EnqueueAsync(ReminderEmailRequested request)
        {
            _queue.Enqueue(request);
            return Task.CompletedTask;
        }
    }
}