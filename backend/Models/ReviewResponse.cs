using System.Text.Json.Serialization;

namespace CodeReviewer.Backend.Models;

public class ReviewResponse
{
    [JsonPropertyName("reviews")]
    public List<ReviewIssue> Reviews { get; set; } = new();

    [JsonPropertyName("rawCodeOrDiff")]
    public string RawCodeOrDiff { get; set; } = string.Empty;
}
