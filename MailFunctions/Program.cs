using Application.Configuration;
using Azure.Communication.Email;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Application.Interfaces;
using Application.Services;
using Infrastructure.Data;
using Infrastructure.Email;
using Infrastructure.Repositories;
using MailFunctions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context, services) =>
    {
        services.Configure<AppOptions>(
            context.Configuration.GetSection("App"));

        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        var configuration = context.Configuration;

        var acsConnectionString = configuration["AcsEmailConnectionString"]
            ?? throw new InvalidOperationException("Missing AcsEmailConnectionString");

        var acsEmailFrom = configuration["AcsEmailFrom"]
            ?? throw new InvalidOperationException("Missing AcsEmailFrom");

        services.AddSingleton(new EmailClient(acsConnectionString));

        services.AddSingleton<MailFunctions.Services.IEmailSender>(sp =>
        {
            var client = sp.GetRequiredService<EmailClient>();
            return new AzureEmailSender(client, acsEmailFrom);
        });

        services.AddSingleton<ITemplateLoader, TemplateLoader>();
        services.AddSingleton<ITemplateRenderer, TemplateRenderer>();
        services.AddSingleton<IEmailSubjectProvider, EmailSubjectProvider>();

        var sqlConnectionString = configuration["SqlConnectionString"];
        services.AddDbContext<RegistrationDbContext>(options =>
        {
            if (string.IsNullOrWhiteSpace(sqlConnectionString))
            {
                options.UseInMemoryDatabase("RegistrationMailFunctions");
            }
            else
            {
                options.UseSqlServer(sqlConnectionString, sql => sql.EnableRetryOnFailure());
            }
        });

        var serviceBusOptions = new ServiceBusOptions
        {
            FullyQualifiedNamespace =
                configuration["ServiceBus__fullyQualifiedNamespace"]
                ?? throw new InvalidOperationException("Missing ServiceBus__fullyQualifiedNamespace"),

            RegistrationCompletedQueueName =
                configuration["RegistrationCompletedQueue"]
                ?? throw new InvalidOperationException("Missing RegistrationCompletedQueue"),

            ReminderQueueName =
                configuration["ReminderQueue"]
                ?? throw new InvalidOperationException("Missing ReminderQueue")
        };

        services.AddSingleton<IOptions<ServiceBusOptions>>(Options.Create(serviceBusOptions));

        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<ServiceBusOptions>>().Value;
            return new ServiceBusClient(options.FullyQualifiedNamespace, new DefaultAzureCredential());
        });

        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<IRegistrationRepository, RegistrationRepository>();
        services.AddSingleton<IEmailDispatcher, ServiceBusDispatcher>();
    })
    .Build();

host.Run();
