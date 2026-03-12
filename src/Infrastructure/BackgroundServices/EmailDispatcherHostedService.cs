using Infrastructure.Email;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.BackgroundServices;

public class EmailDispatcherHostedService : BackgroundService
{
    private readonly InMemoryEmailQueue _queue;
    private readonly IEmailRateLimiter _rateLimiter;

    public EmailDispatcherHostedService(InMemoryEmailQueue queue, IEmailRateLimiter rateLimiter)
    {
        _queue = queue;
        _rateLimiter = rateLimiter;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
            if (_queue.TryDequeue(out var msg))
                await _rateLimiter.ProcessAsync(msg, stoppingToken);
    
    }
}
