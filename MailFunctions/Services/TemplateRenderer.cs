using Contracts;

namespace MailFunctions.Services
{
    public interface ITemplateRenderer
    {
        public string Render(string template, RegistrationCompletedEmailRequested model);
        public string Render(string template, ReminderEmailRequested model);
    }
    public class TemplateRenderer : ITemplateRenderer
    {
        public string Render(string template, RegistrationCompletedEmailRequested model)
        {
            return template
                .Replace("{{AssignedNumber}}", model.AssignedNumber.ToString());
        }
        public string Render(string template, ReminderEmailRequested model)
        {
            return template
                .Replace("{{ResumeLink}}", model.ResumeLink.ToString());
        }
    }
}
