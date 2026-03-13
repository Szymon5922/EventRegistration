using Azure.Communication.Email;
using MailFunctions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context, services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        var configuration = context.Configuration;

        var acsConnectionString = configuration["AcsEmailConnectionString"]
            ?? throw new InvalidOperationException("Missing AcsEmailConnectionString");

        var acsEmailFrom = configuration["AcsEmailFrom"]
            ?? throw new InvalidOperationException("Missing AcsEmailFrom");

        services.AddSingleton(new EmailClient(acsConnectionString));

        services.AddSingleton<IEmailSender>(sp =>
        {
            var client = sp.GetRequiredService<EmailClient>();
            return new AzureEmailSender(client, acsEmailFrom);
        });

        services.AddSingleton<ITemplateLoader, TemplateLoader>();
        services.AddSingleton<ITemplateRenderer, TemplateRenderer>();
        services.AddSingleton<IEmailSubjectProvider, EmailSubjectProvider>();
    })
    .Build();

host.Run();
