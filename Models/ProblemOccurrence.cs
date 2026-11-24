namespace PulseMvc.Models;

public class ProblemOccurrence
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public string ProblemId { get; set; } = string.Empty;
    public string OccurrenceDate { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd");
    public string Note { get; set; } = string.Empty;
}
