using System.ComponentModel.DataAnnotations;

namespace PulseMvc.Models.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Please enter your username or email address.")]
    [Display(Name = "Email or Username")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your password.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long.")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}
