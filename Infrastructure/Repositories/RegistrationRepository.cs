using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class RegistrationRepository(RegistrationDbContext dbContext) : IRegistrationRepository
{
    private readonly RegistrationDbContext _dbContext = dbContext;

    public async Task<RegistrationData?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.Registrations.FindAsync(id, ct);
    }

    public async Task<RegistrationData?> GetByResumeTokenAsync(string resumeToken, CancellationToken ct = default)
    {
        return await _dbContext.Registrations
            .FirstOrDefaultAsync(r => r.ResumeToken == resumeToken, ct);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
    {
        return await _dbContext.Registrations
            .AnyAsync(r => r.Email.Address == email, ct);
    }

    public async Task AddAsync(RegistrationData registrationData, CancellationToken ct = default)
    {
        await _dbContext.Registrations.AddAsync(registrationData, ct);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(RegistrationData registrationData, CancellationToken ct)
    {
        _dbContext.Registrations.Update(registrationData);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<RegistrationData>> GetIncompleteOlderThanAsync(DateTime cutoff, CancellationToken ct = default)
    {
        var list = await _dbContext.Registrations
                                                      .AsNoTracking()
                                                      .Where(r => !r.IsCompleted &&
                                                                  r.LastEditedAt != null &&
                                                                  r.LastEditedAt < cutoff)
                                                      .ToListAsync(ct);

        return list;
    }
    public async Task<bool> TryMarkReminderSentAsync(Guid id, DateTime now, CancellationToken ct = default)
    {
        var affected = await _dbContext.Registrations
            .Where(r => r.Id == id && r.LastReminderSentAt == null)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.LastReminderSentAt, now), ct);

        return affected == 1;
    }
}
