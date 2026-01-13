using System.ComponentModel.DataAnnotations;

namespace Registration.Models;

public class RegistrationStep2ViewModel
{
    public string ResumeToken { get; set; } = default!;
    [Required, Range(16, 120)]
    public int Age { get; set; }

    [Required]
    public string City { get; set; }

    [Required]
    public string PostalCode { get; set; }
}
