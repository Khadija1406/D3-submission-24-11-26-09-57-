namespace PulseMvc.Models;

public class Problem
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = "technical"; // technical, personal, managerial, academic, household, lifestyle
    public string Status { get; set; } = "active"; // active, progress, resolved
    public string Impact { get; set; } = "Medium"; // High, Medium, Low
    public string CreatedAt { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd");
    public string UpdatedAt { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd");

    // Navigation collections for Dapper joins
    public List<ProblemOccurrence> Occurrences { get; set; } = new();
    public List<ProblemTriedSolution> TriedSolutions { get; set; } = new();
}
