using Application.Configuration;
using Application.Interfaces;
using Contracts;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MailFunctions.Functions;

public class ReminderScanFunction
{
    private readonly ILogger _logger;
    private readonly IRegistrationRepository _repository;
    private readonly IEmailDispatcher _emailDispatcher;
    private readonly AppOptions _options;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ReminderScanFunction(ILoggerFactory loggerFactory, 
                                IRegistrationRepository repository, 
                                IDateTimeProvider dateTimeProvider, 
                                IEmailDispatcher emailDispatcher, 
                                IOptions<AppOptions> options)
    {
        _logger = loggerFactory.CreateLogger<ReminderScanFunction>();
        _repository = repository;
        _dateTimeProvider = dateTimeProvider;
        _emailDispatcher = emailDispatcher;
        _options = options.Value;
    }

    [Function("ReminderScanFunction")]
    public async Task Run([TimerTrigger("0 0 12 * * *")] TimerInfo myTimer, CancellationToken cancellationToken)
    {
        _logger.LogInformation("ReminderScanFunction trigger function executed at: {executionTime}", _dateTimeProvider.Now);

        var now = _dateTimeProvider.Now;
        var cutoffDate = now.Date.AddDays(-2);

        var registrationsToRemind = await _repository.GetIncompleteOlderThanAsync(cutoffDate, cancellationToken);

        if (!registrationsToRemind.Any())
        {
            _logger.LogInformation("No registrations to remind.");
            return;
        }

        List<Guid> remindedRegistrations = new();

        try
        {
            foreach (var registration in registrationsToRemind)
            {
                await _emailDispatcher.EnqueueAsync(
                    new ReminderEmailRequested(
                        Guid.NewGuid(),
                        registration.Id, 
                        registration.Email.Address,
                        $"{_options.BaseUrl}/registration/resume/{registration.ResumeToken}",
                        "pl-PL"));

                remindedRegistrations.Add(registration.Id);
            }
        }
        finally
        {
            await _repository.MarkReminderSentAsync(remindedRegistrations, now, cancellationToken);
        }
    }
}