namespace Contracts;

public record RegistrationCompletedEmailRequested(Guid MessageId,
                                                  Guid RegistrationId, 
                                                  string Recipient, 
                                                  int AssignedNumber, 
                                                  DateTime CreatedAtUtc, 
                                                  string Locale) : IEmailRequest;
