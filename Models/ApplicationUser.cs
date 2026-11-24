using Microsoft.AspNetCore.Identity;

namespace PulseMvc.Models;

/// <summary>
/// Customized Identity User for Pulse application
/// </summary>
public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? AvatarUrl { get; set; }
}
