using Domain.Entities;

namespace Application.Interfaces
{
    public interface IRegistrationRepository
    {
        Task<RegistrationData?> GetByIdAsync(Guid id, CancellationToken ct);
        Task<RegistrationData?> GetByResumeTokenAsync(string token, CancellationToken ct);
        Task<bool> EmailExistsAsync(string email, CancellationToken ct);

        Task AddAsync(RegistrationData registration, CancellationToken ct);
        Task UpdateAsync(RegistrationData registration, CancellationToken ct);

        Task<IReadOnlyList<RegistrationData>> GetIncompleteOlderThanAsync(DateTime cutoff, CancellationToken ct);
        Task MarkReminderSentAsync(IEnumerable<Guid> ids, DateTime now, CancellationToken ct);
    }
}
