using Application.Configuration;
using Application.Interfaces;
using Application.Services;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Infrastructure.Data;
using Infrastructure.Email;
using Infrastructure.Repositories;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

//Key Vault
if(!builder.Environment.IsDevelopment())
{
    var keyVaultUri = builder.Configuration["KeyVault:Uri"];
    if (!string.IsNullOrWhiteSpace(keyVaultUri))
        builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUri), new DefaultAzureCredential());
}
// MVC
builder.Services.AddControllersWithViews();


//Insights
builder.Services.AddApplicationInsightsTelemetry();

// Serilog
builder.Host.UseSerilog((ctx, services, lc) =>
{
    var telemetryConfig = services.GetRequiredService<TelemetryConfiguration>();

    lc.MinimumLevel.Information()
      .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
      .Enrich.FromLogContext()
      .WriteTo.Console()
      .WriteTo.ApplicationInsights(telemetryConfig, TelemetryConverter.Traces);
});

// Infrastructure - data access
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDbContext<RegistrationDbContext>(options =>
        options.UseInMemoryDatabase("RegistrationDev"));
}
else
{
    var connectionString =
        builder.Configuration["SqlConnectionString"]
        ?? throw new InvalidOperationException("SQL connection string not found.");

    builder.Services.AddDbContext<RegistrationDbContext>(options =>
        options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));
}

builder.Services.AddScoped<IRegistrationRepository, RegistrationRepository>();

// Service bus
builder.Services.Configure<ServiceBusOptions>(builder.Configuration.GetSection("ServiceBus"));
builder.Services.AddSingleton(sp =>
{
    var options = sp.GetRequiredService<IOptions<ServiceBusOptions>>().Value;

    return new ServiceBusClient(
        options.FullyQualifiedNamespace,
        new DefaultAzureCredential());
});

// Infrastructure – email subsystem
//builder.Services.AddSingleton<InMemoryEmailQueue>();
//builder.Services.AddSingleton<IEmailDispatcher, InMemoryEmailDispatcher>();
builder.Services.AddSingleton<IEmailDispatcher, ServiceBusDispatcher>();
//builder.Services.AddSingleton<IEmailSender, FakeEmailSender>();


// Application
builder.Services.Configure<AppOptions>(builder.Configuration.GetSection("App"));
builder.Services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
builder.Services.AddScoped<IRegistrationService, RegistrationService>();

var app = builder.Build();

// Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();

app.MapStaticAssets();

app.UseRouting();
app.UseAuthorization();

// Routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Registration}/{action=Start}/{id?}")
    .WithStaticAssets();

app.Run();
