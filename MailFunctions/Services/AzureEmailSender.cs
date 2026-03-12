using Azure;
using Azure.Communication.Email;
using MailFunctions.Interfaces;
using MailFunctions.Models;

namespace MailFunctions.Services
{
    public class AzureEmailSender : IEmailSender
    {
        private readonly EmailClient _client;
        private readonly string _from;

        public AzureEmailSender(EmailClient client, string from)
        {
            _client = client;
            _from = from;
        }

        public async Task<bool> SendSingleAsync(Models.EmailMessage message)
        {
            var email = new Azure.Communication.Email.EmailMessage(
                senderAddress: _from,
                content: new EmailContent(message.Subject)
                {
                    Html = message.Body
                },
                recipients: new EmailRecipients(
                    new List<EmailAddress>
                    {
                    new EmailAddress(message.To)
                    }));

            EmailSendOperation op =
                await _client.SendAsync(WaitUntil.Completed, email);

            return op.HasCompleted && op.Value.Status == EmailSendStatus.Succeeded;
        }
    }
}
