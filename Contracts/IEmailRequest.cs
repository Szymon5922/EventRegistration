namespace Contracts
{
    public interface IEmailRequest
    {
        Guid MessageId { get; }
        Guid RegistrationId { get; }
        string Recipient { get; }
        string Locale { get; }
    }
}
