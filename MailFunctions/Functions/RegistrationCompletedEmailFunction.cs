using Azure.Messaging.ServiceBus;
using Contracts;
using MailFunctions.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MailFunctions.Functions
{
    public class RegistrationCompletedEmailFunction
    {
        private readonly IEmailSender _emailSender;        
        private readonly ITemplateLoader _templateLoader;
        private readonly ITemplateRenderer _templateRenderer;
        private readonly ILogger<RegistrationCompletedEmailFunction> _logger;

        public RegistrationCompletedEmailFunction(IEmailSender emailSender, ILogger<RegistrationCompletedEmailFunction> logger)
        {
            _emailSender = emailSender;
            _logger = logger;
            
        }

        [Function(nameof(RegistrationCompletedEmailFunction))]
        public async Task Run(
            [ServiceBusTrigger("registration-completed-emails", Connection = "ServiceBus")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            var request = JsonSerializer.Deserialize<RegistrationCompletedEmailRequested>(
    message.Body.ToString());

            if (request == null)
                throw new InvalidOperationException("Invalid message payload");

            var subjectTemplate = _templateLoader.Load("registration-completed.subject.txt");
            var bodyTemplate = _templateLoader.Load("registration-completed.body.html");

            var subject = _templateRenderer.Render(subjectTemplate, request);
            var body = _templateRenderer.Render(bodyTemplate, request);

            var email = new Models.EmailMessage(
                To: request.Recipient,
                Subject: subject,
                Body: body);

            await _emailSender.SendSingleAsync(email);

            _logger.LogInformation(
                "Registration completed email sent for registration {registrationId}",
                request.RegistrationId);
        }
    }
}
