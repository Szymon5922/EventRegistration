using Domain.ValueObjects;

namespace Domain.Entities
{
    public class RegistrationData
    {
        public Guid Id { get; private set; }
        public Email Email { get; private set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? Age { get; set; }
        public bool? IsMinor { get; set; }
        public string? City { get; set; }
        public string? PostalCode { get; set; }
        public string? ParentName { get; set; }
        public bool? ConsentGiven { get; set; }

        public string ClientIp { get; set; }
        public int CurrentStep { get; set; }
        public string ResumeToken { get; set; }

        public DateTime CreatedAt { get; private set; }
        public DateTime? LastEditedAt { get; set; }

        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int? AssignedNumber { get; set; }

        public DateTime? LastReminderSentAt { get; set; }

        private RegistrationData() { }

        public RegistrationData(Guid id, 
                                Email email, 
                                string clientIp, 
                                string firstName, 
                                string lastName, 
                                DateTime createdAt,
                                string resumeToken)
        {
            Id = id;
            Email = email;
            ClientIp = clientIp;
            FirstName = firstName;
            LastName = lastName;
            CreatedAt = createdAt;
            LastEditedAt = createdAt;
            ResumeToken = resumeToken;
            CurrentStep = 1;
            IsCompleted = false;
        }
    }
}
