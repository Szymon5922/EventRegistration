using Application.Models;
using System.Collections.Concurrent;

namespace Infrastructure.Email
{
    public class EmailQueue
    {
        private readonly ConcurrentQueue<EmailMessage> _queue = new();

        public void Enqueue(EmailMessage message)
            => _queue.Enqueue(message);

        public bool TryDequeue(out EmailMessage? message)
            => _queue.TryDequeue(out message);

        public int Count => _queue.Count;
    }
}