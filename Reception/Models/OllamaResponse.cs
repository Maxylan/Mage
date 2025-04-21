using System.Text.Json;

namespace Reception.Models;

public class OllamaResponse
{
    public class Analysis
    {
        // 'summary' (string, 20-100 characters) - Brief, easily indexable (..but still human readable..) summary of your content analysis findings
        // [JsonProperty("summary", NullValueHandling = NullValueHandling.Ignore)]
        public string? Summary { get; set; }

        // 'description' (string, 80-400 characters) - A human-readable description of image contents.
        // [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string? Description { get; set; }

        // 'tags' (string[], 4-16 items) - Array of single-word tags (strings) that index / categorize the image & its contents.
        // [JsonProperty("tags", NullValueHandling = NullValueHandling.Ignore)]
        public string[]? Tags { get; set; }
    }

    // The generated text content
    // [JsonProperty("response", NullValueHandling = NullValueHandling.Ignore)]
    public string? Response { get; set; }

    // Indicates whether generation has completed
    // [JsonProperty("done")]
    public bool Done { get; set; }

    // Echoed back model name that was used
    // [JsonProperty("model")]
    public string Model { get; set; } = null!;

    // Timestamp (or equivalent string) when generation occurred
    // [JsonProperty("created_at")]
    public string CreatedAt { get; set; } = null!;

    // An identifier for the log or session (if provided)
    // [JsonProperty("log_id", NullValueHandling = NullValueHandling.Ignore)]
    public string LogId { get; set; } = null!;

    // Optional: error message if something went wrong
    // [JsonProperty("error", NullValueHandling = NullValueHandling.Ignore)]
    public string Error { get; set; } = null!;
}
