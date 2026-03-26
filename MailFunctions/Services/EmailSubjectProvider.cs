using System.Globalization;
using System.Resources;

namespace MailFunctions.Services
{
    public interface IEmailSubjectProvider
    {
        string GetRegistrationCompletedSubject(string locale);
        string GetReminderSubject(string locale);
    }

    public class EmailSubjectProvider : IEmailSubjectProvider
    {
        private static readonly ResourceManager ResourceManager =
            new("MailFunctions.Resources.EmailSubjects", typeof(EmailSubjectProvider).Assembly);

        public string GetRegistrationCompletedSubject(string locale)
        {
            var culture = new CultureInfo(locale);
            return ResourceManager.GetString("RegistrationCompleted", culture)
                   ?? throw new InvalidOperationException("Missing subject resource: RegistrationCompleted");
        }
        public string GetReminderSubject(string locale)
        {
            var culture = new CultureInfo(locale);
            return ResourceManager.GetString("RegistrationReminder", culture)
                   ?? throw new InvalidOperationException("Missing subject resource: RegistrationReminder");
        }
    }
}
