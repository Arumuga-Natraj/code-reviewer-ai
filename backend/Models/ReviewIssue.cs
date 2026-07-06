using System.Text.Json.Serialization;

namespace CodeReviewer.Backend.Models;

public class ReviewIssue
{
    [JsonPropertyName("filePath")]
    public string FilePath { get; set; } = string.Empty;

    [JsonPropertyName("originalCode")]
    public string OriginalCode { get; set; } = string.Empty;

    [JsonPropertyName("suggestedCode")]
    public string SuggestedCode { get; set; } = string.Empty;

    [JsonPropertyName("comment")]
    public string Comment { get; set; } = string.Empty;

    [JsonPropertyName("lineStart")]
    public int? LineStart { get; set; }

    [JsonPropertyName("lineEnd")]
    public int? LineEnd { get; set; }
}
