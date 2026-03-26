using Application.Configuration;
using Application.Interfaces;
using Azure.Messaging.ServiceBus;
using Contracts;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Infrastructure.Email
{
    public class ServiceBusDispatcher : IEmailDispatcher, IAsyncDisposable
    {
        private readonly ServiceBusSender _registrationCompletedSender;
        private readonly ServiceBusSender _reminderSender;

        public ServiceBusDispatcher(ServiceBusClient client, IOptions<ServiceBusOptions> options)
        {
            _registrationCompletedSender = client.CreateSender(options.Value.RegistrationCompletedQueueName);
            _reminderSender = client.CreateSender(options.Value.ReminderQueueName);
        }

        public async Task EnqueueAsync(RegistrationCompletedEmailRequested request)
        {
            var message = CreateMessage(request);
            await _registrationCompletedSender.SendMessageAsync(message);
        }

        public async Task EnqueueAsync(ReminderEmailRequested request)
        {
            var message = CreateMessage(request);
            await _reminderSender.SendMessageAsync(message);
        }
        private ServiceBusMessage CreateMessage(IEmailRequest request)
        {
            var json = JsonSerializer.Serialize(request);
            var message = new ServiceBusMessage(json)
            {
                MessageId = request.MessageId.ToString(),
                ContentType = "application/json",
            };

            message.ApplicationProperties["messageType"] =
                request.GetType().Name;

            message.ApplicationProperties["registrationId"] =
                request.RegistrationId.ToString();

            return message;
        }
        public async ValueTask DisposeAsync()
        {
            await _registrationCompletedSender.DisposeAsync();
            await _reminderSender.DisposeAsync();
        }
    }
}
