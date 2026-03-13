using Azure.Messaging.ServiceBus;
using Contracts;
using MailFunctions.Services;
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
        private readonly IEmailSubjectProvider _subjectProvider;
        private readonly ILogger<RegistrationCompletedEmailFunction> _logger;

        public RegistrationCompletedEmailFunction(IEmailSender emailSender, 
                                                  ITemplateLoader templateLoader, 
                                                  ITemplateRenderer templateRenderer, 
                                                  IEmailSubjectProvider subjectProvider, 
                                                  ILogger<RegistrationCompletedEmailFunction> logger)
        {
            _emailSender = emailSender;
            _templateLoader = templateLoader;
            _templateRenderer = templateRenderer;
            _subjectProvider = subjectProvider;
            _logger = logger;
        }

        [Function(nameof(RegistrationCompletedEmailFunction))]
        public async Task Run(
            [ServiceBusTrigger("%RegistrationCompletedQueue%", Connection = "ServiceBus")]
            ServiceBusReceivedMessage message)
        {
            var request = JsonSerializer.Deserialize<RegistrationCompletedEmailRequested>(
    message.Body.ToString());

            if (request == null)
                throw new InvalidOperationException("Invalid message payload");

            var subject = _subjectProvider.GetRegistrationCompletedSubject(request.Locale);
            var bodyTemplate = _templateLoader.GetRegistrationCompletedBodyTemplate(request.Locale);
            var body = _templateRenderer.Render(bodyTemplate, request);

            var email = new Models.EmailMessage(
                To: request.Recipient,
                Subject: subject,
                Body: body);

            var sent = await _emailSender.SendSingleAsync(email);

            if (!sent)
                throw new InvalidOperationException("Email sender reported failure.");

            _logger.LogInformation(
                "Registration completed email sent for registration {RegistrationId}",
                request.RegistrationId);
        }
    }
}
