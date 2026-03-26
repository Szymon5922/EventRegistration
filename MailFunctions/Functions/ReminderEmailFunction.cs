using Azure.Messaging.ServiceBus;
using Contracts;
using MailFunctions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace MailFunctions.Functions
{
    public class ReminderEmailFunction : EmailFunctionBase<ReminderEmailRequested>
    {
        private readonly ITemplateLoader _templateLoader;
        private readonly ITemplateRenderer _templateRenderer;
        private readonly IEmailSubjectProvider _subjectProvider;

        public ReminderEmailFunction(IEmailSender emailSender,
                                     ITemplateLoader templateLoader,
                                     ITemplateRenderer templateRenderer,
                                     IEmailSubjectProvider subjectProvider,
                                     ILogger<ReminderEmailFunction> logger) 
            : base(emailSender, logger)
        {
            _templateLoader = templateLoader;
            _templateRenderer = templateRenderer;
            _subjectProvider = subjectProvider;
        }

        [Function(nameof(ReminderEmailFunction))]
        public async Task Run([ServiceBusTrigger("%ReminderQueue%", Connection = "ServiceBus")] ServiceBusReceivedMessage message)
        {
            SetRequest(message);
            await Handle();
        }

        protected override string CreateBody()
        {
            var bodyTemplate = _templateLoader.GetRegistrationReminderBodyTemplate(Request.Locale);
            return _templateRenderer.Render(bodyTemplate, Request);
        }

        protected override string CreateSubject()
        {
            return _subjectProvider.GetReminderSubject(Request.Locale);
        }
    }
}
