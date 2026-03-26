using System.ComponentModel.DataAnnotations;

namespace Registration.Models;

public class RegistrationStep1ViewModel
{
    [Required]
    public string FirstName { get; set; }

    [Required]
    public string LastName { get; set; }

    [Required, EmailAddress]
    public string Email { get; set; }
}
