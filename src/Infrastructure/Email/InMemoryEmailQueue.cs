using Contracts;
using System.Collections.Concurrent;

namespace Infrastructure.Email
{
    public class InMemoryEmailQueue
    {
        private readonly ConcurrentQueue<RegistrationCompletedEmailRequested> _completedQueue = new ConcurrentQueue<RegistrationCompletedEmailRequested>();
        private readonly ConcurrentQueue<ReminderEmailRequested> _reminderQueue = new();

        public void Enqueue(RegistrationCompletedEmailRequested message)
            => _completedQueue.Enqueue(message);

        public bool TryDequeue(out RegistrationCompletedEmailRequested? message)
            => _completedQueue.TryDequeue(out message);

        public void Enqueue(ReminderEmailRequested message)
            => _reminderQueue.Enqueue(message);

        public bool TryDequeue(out ReminderEmailRequested? message)
            => _reminderQueue.TryDequeue(out message);
    }
}