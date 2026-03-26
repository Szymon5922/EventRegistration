using Domain.ValueObjects;

namespace Application.Models
{
    public class ImportUserModel
    {
        public Email Email { get; private set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public int? Age { get; set; }
        public string? City { get; set; }
        public string? PostalCode { get; set; }
        public string? ParentName { get; set; }
        public bool? ConsentGiven { get; set; }
        public string ClientIp { get; set; }
    }
}
