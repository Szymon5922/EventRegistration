using Azure.Core;
using Azure.Messaging.ServiceBus;
using Contracts;
using MailFunctions.Services;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MailFunctions.Functions
{
    public abstract class EmailFunctionBase<T> where T : IEmailRequest
    {
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        protected T Request { get; private set; }

        protected EmailFunctionBase(IEmailSender emailSender, ILogger logger)
        {
            _emailSender = emailSender;
            _logger = logger;
        }
        protected void SetRequest(ServiceBusReceivedMessage message)
        {
            Request = JsonSerializer.Deserialize<T>(message.Body.ToString())
                ?? throw new InvalidOperationException("Invalid message payload.");
        }
        public async Task Handle()
        {
            if (Request == null)
                throw new InvalidOperationException("Invalid message payload or request not set");

            var subject = CreateSubject();
            var body = CreateBody();

            var email = new Models.EmailMessage(
                To: Request.Recipient,
                Subject: subject,
                Body: body);

            var sent = await _emailSender.SendSingleAsync(email);

            if (!sent)
                throw new InvalidOperationException($"Email sender reported failure.");

            _logger.LogInformation($"Email sent for {Request.GetType().Name} {Request.RegistrationId}");
        }

        protected abstract string CreateSubject();
        protected abstract string CreateBody();
    }
}
