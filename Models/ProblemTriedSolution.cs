namespace PulseMvc.Models;

public class ProblemTriedSolution
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public string ProblemId { get; set; } = string.Empty;
    public string SolutionKey { get; set; } = string.Empty;
    public string Status { get; set; } = "partial"; // worked, partial, failed
    public string Note { get; set; } = string.Empty;
}
