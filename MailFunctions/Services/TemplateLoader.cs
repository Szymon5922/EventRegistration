namespace MailFunctions.Services
{
    public interface ITemplateLoader
    {
        public string GetRegistrationCompletedBodyTemplate(string locale);
        public string GetRegistrationReminderBodyTemplate(string locale);
    }
    public class TemplateLoader : ITemplateLoader
    {
        public string GetRegistrationCompletedBodyTemplate(string locale) => GetTemplate("registration-complete", locale);

        public string GetRegistrationReminderBodyTemplate(string locale) => GetTemplate("reminder", locale);
        private string GetTemplate(string templateName, string locale)
        {
            var fullPath = Path.Combine(AppContext.BaseDirectory, "Resources", $"{templateName}.body.{locale}.html");

            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"Template file not found: {fullPath}");

            return File.ReadAllText(fullPath);
        }
    }
}
