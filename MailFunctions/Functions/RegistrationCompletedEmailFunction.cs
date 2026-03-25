using Azure.Messaging.ServiceBus;
using Contracts;
using MailFunctions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace MailFunctions.Functions
{
    public class RegistrationCompletedEmailFunction : EmailFunctionBase <RegistrationCompletedEmailRequested>
    {
        private readonly ITemplateLoader _templateLoader;
        private readonly ITemplateRenderer _templateRenderer;
        private readonly IEmailSubjectProvider _subjectProvider;
        private readonly ILogger<RegistrationCompletedEmailFunction> _logger;

        public RegistrationCompletedEmailFunction(IEmailSender emailSender, 
                                                  ITemplateLoader templateLoader, 
                                                  ITemplateRenderer templateRenderer, 
                                                  IEmailSubjectProvider subjectProvider, 
                                                  ILogger<RegistrationCompletedEmailFunction> logger) 
            : base(emailSender, logger)
        {
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
            SetRequest(message);
            await Handle();
        }

        protected override string CreateBody()
        {
            var bodyTemplate = _templateLoader.GetRegistrationCompletedBodyTemplate(Request.Locale);
            return _templateRenderer.Render(bodyTemplate, Request);
        }

        protected override string CreateSubject()
        {
            return _subjectProvider.GetRegistrationCompletedSubject(Request.Locale);
        }
    }
}
