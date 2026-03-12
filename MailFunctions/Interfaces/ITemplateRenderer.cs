using Contracts;

namespace MailFunctions.Interfaces
{
    public interface ITemplateRenderer
    {
        public string Render(string template, RegistrationCompletedEmailRequested model);
        public string Render(string template, ReminderEmailRequested model);
    }
}
