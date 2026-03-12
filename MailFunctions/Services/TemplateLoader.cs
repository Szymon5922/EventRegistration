namespace MailFunctions.Services
{
    public class TemplateLoader
    {
        public string Load(string relativePath)
        {
            var fullPath = Path.Combine(AppContext.BaseDirectory, relativePath);

            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"Template file not found: {fullPath}");

            return File.ReadAllText(fullPath);
        }
    }
}
