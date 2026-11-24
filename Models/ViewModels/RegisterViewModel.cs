using System.ComponentModel.DataAnnotations;

namespace PulseMvc.Models.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Please enter your name or username.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 50 characters.")]
    [Display(Name = "Your name")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter a valid email address.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    [Display(Name = "Email address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long.")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm your password.")]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
