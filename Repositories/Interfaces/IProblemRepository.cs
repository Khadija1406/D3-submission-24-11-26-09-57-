using PulseMvc.Models;
using PulseMvc.Models.ViewModels;

namespace PulseMvc.Repositories.Interfaces;

public interface IProblemRepository
{
    Task<IEnumerable<ProblemDto>> GetAllAsync(string? userId = null);
    Task<ProblemDto?> GetByIdAsync(string id);
    Task<string> CreateAsync(CreateProblemRequest request, string userId);
    Task<bool> UpdateStatusAsync(string id, string status);
    Task<bool> AddOccurrenceAsync(string problemId, string date, string note);
    Task<bool> SetTriedSolutionAsync(string problemId, string solutionKey, string status, string note);
    Task<bool> DeleteAsync(string id);
}
