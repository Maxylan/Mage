using System.Text.Json;

namespace Reception.Models;

public class OllamaResponse
{
    // The generated text content
    // [JsonProperty("response")]
    public string Response { get; set; }

    // Indicates whether generation has completed
    // [JsonProperty("done")]
    public bool Done { get; set; }

    // Echoed back model name that was used
    // [JsonProperty("model")]
    public string Model { get; set; }

    // Timestamp (or equivalent string) when generation occurred
    // [JsonProperty("created_at")]
    public string CreatedAt { get; set; }

    // An identifier for the log or session (if provided)
    // [JsonProperty("log_id", NullValueHandling = NullValueHandling.Ignore)]
    public string LogId { get; set; }

    // Optional: error message if something went wrong
    // [JsonProperty("error", NullValueHandling = NullValueHandling.Ignore)]
    public string Error { get; set; }
}
