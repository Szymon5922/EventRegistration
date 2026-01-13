using Application.Models;

namespace Infrastructure.Email;

public class ThrottledRateLimiter : IEmailRateLimiter
{
    private readonly IEmailSender _sender;

    private readonly List<EmailMessage> _buffer = new(5);
    private readonly object _lock = new();

    private DateTime? _firstMessageTimestamp = null;

    private readonly TimeSpan _maxWait = TimeSpan.FromMilliseconds(1000);

    public ThrottledRateLimiter(IEmailSender sender)
    {
        _sender = sender;
    }

    public async Task ProcessAsync(EmailMessage msg, CancellationToken token)
    {
        List<EmailMessage>? toSend = null;

        lock (_lock)
        {
            _buffer.Add(msg);

            if (_buffer.Count == 1)
                _firstMessageTimestamp = DateTime.UtcNow;

            if (_buffer.Count == 5)
            {
                toSend = new List<EmailMessage>(_buffer);
                _buffer.Clear();
                _firstMessageTimestamp = null;
            }
            else
            {
                if (_firstMessageTimestamp.HasValue &&
                    DateTime.UtcNow - _firstMessageTimestamp.Value > _maxWait)
                {
                    toSend = new List<EmailMessage>(_buffer);
                    _buffer.Clear();
                    _firstMessageTimestamp = null;
                }
            }
        }

        if (toSend == null)
            return;

        //throttling implementation...

        if (toSend.Count == 1)
        {
            await _sender.SendSingleAsync(toSend[0]);
        }
        else
        {
            await _sender.SendBatchAsync(toSend);
        }
    }
}
