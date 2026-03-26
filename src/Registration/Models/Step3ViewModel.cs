using System.ComponentModel.DataAnnotations;

namespace Registration.Models;

public class RegistrationStep3ViewModel
{
    public string ResumeToken { get; set; } = default!;
    public bool IsMinor {  get; set; }
    public string? ParentName { get; set; }

    [Required(ErrorMessage = "Musisz zaakceptować zgodę.")]
    public bool ConsentGiven { get; set; }
}
