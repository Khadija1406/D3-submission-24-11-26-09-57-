using System.Text.Json.Serialization;

namespace PulseMvc.Models.ViewModels;

public class ProblemDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("userId")]
    public string? UserId { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("desc")]
    public string Desc { get; set; } = string.Empty;

    [JsonPropertyName("cat")]
    public string Cat { get; set; } = "technical";

    [JsonPropertyName("status")]
    public string Status { get; set; } = "active";

    [JsonPropertyName("impact")]
    public string Impact { get; set; } = "Medium";

    [JsonPropertyName("created")]
    public string Created { get; set; } = string.Empty;

    [JsonPropertyName("occ")]
    public List<OccurrenceDto> Occ { get; set; } = new();

    [JsonPropertyName("tried")]
    public Dictionary<string, TriedDto> Tried { get; set; } = new();
}

public class OccurrenceDto
{
    [JsonPropertyName("d")]
    public string D { get; set; } = string.Empty;

    [JsonPropertyName("n")]
    public string N { get; set; } = string.Empty;
}

public class TriedDto
{
    [JsonPropertyName("s")]
    public string S { get; set; } = "partial"; // worked, partial, failed

    [JsonPropertyName("n")]
    public string N { get; set; } = string.Empty;
}

public class CreateProblemRequest
{
    public string? Title { get; set; }
    public string? Desc { get; set; }
    public string? Cat { get; set; }
    public string? Status { get; set; }
    public string? Impact { get; set; }
    public string? Created { get; set; }
}

public class UpdateStatusRequest
{
    public string Id { get; set; } = string.Empty;
    public string Status { get; set; } = "active";
}

public class AddOccurrenceRequest
{
    public string ProblemId { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
}

public class SetTriedSolutionRequest
{
    public string ProblemId { get; set; } = string.Empty;
    public string SolutionKey { get; set; } = string.Empty;
    public string Status { get; set; } = "partial";
    public string Note { get; set; } = string.Empty;
}
