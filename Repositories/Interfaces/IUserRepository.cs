using PulseMvc.Models;

namespace PulseMvc.Repositories.Interfaces;

public interface IUserRepository
{
    Task<ApplicationUser?> FindByIdAsync(string id);
    Task<ApplicationUser?> FindByEmailAsync(string email);
    Task<ApplicationUser?> FindByNameAsync(string normalizedUserName);
    Task<bool> CreateAsync(ApplicationUser user);
    Task<bool> UpdateAsync(ApplicationUser user);
}
