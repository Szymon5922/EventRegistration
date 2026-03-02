using Application.Configuration;
using Application.Interfaces;
using Application.Services;
using Azure.Identity;
using Infrastructure.BackgroundServices;
using Infrastructure.Data;
using Infrastructure.Email;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

//Key Vault
var keyVaultUri = builder.Configuration["KeyVault:Uri"];
if(!string.IsNullOrEmpty(keyVaultUri))
    builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUri), new DefaultAzureCredential());

// MVC
builder.Services.AddControllersWithViews();

// Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .MinimumLevel.Debug()
    .Enrich.FromLogContext()
    .CreateLogger();
builder.Host.UseSerilog();

// Infrastructure - data access
var connectionString =
    builder.Configuration["SqlConnectionString"]
        ?? throw new InvalidOperationException("SQL connection string not found.");

builder.Services.AddDbContext<RegistrationDbContext>(options =>
    options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));

builder.Services.AddScoped<IRegistrationRepository, RegistrationRepository>();

// Infrastructure – email subsystem
builder.Services.AddSingleton<EmailQueue>();
builder.Services.AddSingleton<IEmailSender, FakeEmailSender>();
builder.Services.AddSingleton<IEmailRateLimiter, ThrottledRateLimiter>();
builder.Services.AddSingleton<IEmailDispatcher, EmailDispatcher>();
builder.Services.AddHostedService<EmailDispatcherHostedService>();
builder.Services.AddHostedService<ReminderWorker>();

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
