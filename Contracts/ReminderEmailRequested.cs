namespace Contracts;

public record ReminderEmailRequested(Guid MessageId,
                                     Guid RegistrationId,
                                     string Recipient,
                                     string ResumeLink,
                                     string Locale) : IEmailRequest;
