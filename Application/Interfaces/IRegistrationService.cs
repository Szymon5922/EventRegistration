using Application.Models;
using CSharpFunctionalExtensions;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IRegistrationService
    {
        Task<Result<string>> StartAsync(Step1Data data, string clientIp, CancellationToken ct);

        Task<Result<RegistrationData>> GetByResumeTokenAsync(string resumeToken, CancellationToken ct);

        Task<Result> SubmitStep2Async(string resumeToken, Step2Data data, CancellationToken ct);

        Task<Result> SubmitStep3AndCompleteAsync(RegistrationData reg, Step3Data data, CancellationToken ct);
    }
}
