namespace Application.Configuration
{
    public sealed class ServiceBusOptions
    {
        public string FullyQualifiedNamespace { get; init; } = null!;
        public string RegistrationCompletedQueueName { get; init; } = null!;
        public string ReminderQueueName { get; init; } = null!;
    }
}
