using Contracts;

namespace Application.Interfaces
{
    public interface IEmailDispatcher
    {
        Task EnqueueAsync(RegistrationCompletedEmailRequested request);
        Task EnqueueAsync(ReminderEmailRequested request);
    }
}
