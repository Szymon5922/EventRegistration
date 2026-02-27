using Application.Interfaces;
using Application.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Email;

public class ReminderWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IEmailDispatcher _dispatcher;
    private readonly IDateTimeProvider _clock;
    private readonly ILogger<ReminderWorker> _logger;

    public ReminderWorker(
        IServiceScopeFactory scopeFactory,
        IEmailDispatcher dispatcher,
        IDateTimeProvider clock,
        ILogger<ReminderWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _dispatcher = dispatcher;
        _clock = clock;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ReminderWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<IRegistrationRepository>();

                await ProcessRemindersAsync(repo, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ReminderWorker loop.");
            }

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    private async Task ProcessRemindersAsync(IRegistrationRepository repo, CancellationToken ct)
    {
        var cutoff = _clock.Now.AddHours(-1);

        var list = await repo.GetIncompleteOlderThanAsync(cutoff, ct);

        if (!list.Any())
            return;

        foreach (var reg in list)
        {
            var marked = await repo.TryMarkReminderSentAsync(reg.Id, _clock.Now, ct);

            if (!marked)
                continue; //sent by other instance

            var link = $"https://localhost:5001/registration/resume/{reg.ResumeToken}";

            var message = new EmailMessage(
                To: reg.Email.Address,
                Subject: "Dokończ rejestrację",
                Body: $"Wygląda na to, że porzuciłeś formularz. Możesz go dokończyć tutaj:\n{link}"
            );

            await _dispatcher.EnqueueAsync(message);

            reg.LastReminderSentAt = _clock.Now;
            await _repo.UpdateAsync(reg, ct);
        }
    }
}
