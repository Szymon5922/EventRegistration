using Application.Common;
using Application.Interfaces;
using Application.Models;
using CSharpFunctionalExtensions;
using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Application.Services;

public class RegistrationService : IRegistrationService
{
    private readonly IRegistrationRepository _repo;
    private readonly IEmailDispatcher _dispatcher;
    private readonly IDateTimeProvider _clock;
    private readonly ILogger<RegistrationService> _logger;
    public RegistrationService(
        IRegistrationRepository repo,
        IEmailDispatcher dispatcher,
        IDateTimeProvider clock,
        ILogger<RegistrationService> logger)

    {
        _repo = repo;
        _dispatcher = dispatcher;
        _clock = clock;
        _logger = logger;
    }

    public async Task<Result<string>> StartAsync(Step1Data data, string clientIp, CancellationToken ct)
    {
        var emailResult = Email.Create(data.Email);
        if (emailResult.IsFailure)
            return Result.Failure<string>(emailResult.Error);

        var email = emailResult.Value;

        if (await _repo.EmailExistsAsync(email.Address, ct))
            return Result.Failure<string>("Email already registered.");

        var registration = new RegistrationData(
            Guid.NewGuid(),
            email,
            clientIp,
            data.FirstName,
            data.LastName,
            _clock.Now,
            TokenGenerator.GenerateResumeToken()
        );

        try
        {
            await _repo.AddAsync(registration, ct);            
        }
        catch (Exception ex)
        {
            return Result.Failure<string>(ex.Message);
        }

        return Result.Success(registration.ResumeToken);
    }

    public async Task<Result> SubmitStep2Async(string resumeToken, Step2Data data, CancellationToken ct)
    {
        var reg = await _repo.GetByResumeTokenAsync(resumeToken, ct);
        if (reg == null)
        {
            _logger.LogWarning($"SubmitStep2 failed: token not found {resumeToken}");
            return Result.Failure("Registration not found.");
        }

        if (reg.IsCompleted)
            return Result.Failure("Registration already completed.");

        if (reg.CurrentStep < 1)
            return Result.Failure("Step 1 must be completed first.");

        if (data.Age < 16)
            return Result.Failure("Minimum age is 16.");

        reg.Age = data.Age;
        reg.City = data.City;
        reg.PostalCode = data.PostalCode;
        reg.LastEditedAt = _clock.Now;
        reg.IsMinor = data.Age < 18;
        reg.CurrentStep = 2;

        await _repo.UpdateAsync(reg, ct);

        return Result.Success();
    }

    public async Task<Result> SubmitStep3AndCompleteAsync(RegistrationData reg, Step3Data data, CancellationToken ct)
    {
        if (reg.IsCompleted)
            return Result.Failure("Registration already completed.");

        if (reg.CurrentStep < 2)
            return Result.Failure("Step 2 must be completed first.");

        if (reg.Age == null)
        {
            _logger.LogWarning($"SubmitStep3 failed: Age not set in Step2 for token {reg.ResumeToken}");

            return Result.Failure("Age must be specified in Step 2.");
        }

        bool isMinor = reg.Age < 18;

        if (isMinor && string.IsNullOrWhiteSpace(data.ParentName))
            return Result.Failure("Parent name required for minors.");

        if (!data.ConsentGiven)
            return Result.Failure("Consent must be accepted.");

        reg.ParentName = data.ParentName;
        reg.ConsentGiven = data.ConsentGiven;

        var now = _clock.Now;
        reg.CompletedAt = now;
        reg.LastEditedAt = now;
        reg.IsCompleted = true;
        reg.CurrentStep = 3;

        if (!reg.AssignedNumber.HasValue)
            reg.AssignedNumber = SequenceGenerator.Next();

        await _repo.UpdateAsync(reg, ct);

        var message = new EmailMessage(
            To: reg.Email.Address,
            Subject: "Dziękujemy za rejestrację",
            Body: $"Twoja rejestracja została pomyślnie ukończona. Numer rejestracji:{reg.AssignedNumber}");

        await _dispatcher.EnqueueAsync(message);

        return Result.Success();
    }
    public async Task<Result<RegistrationData>> GetByResumeTokenAsync(string resumeToken, CancellationToken ct)
    {
        var reg = await _repo.GetByResumeTokenAsync(resumeToken, ct);
        if (reg == null)
            return Result.Failure<RegistrationData>("Registration not found.");

        return Result.Success(reg);
    }
}
