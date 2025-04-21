using System.Text.Json;

namespace Reception.Models;

public class OllamaAnalysis : OllamaResponse
{
    // The generated text content
    // [JsonProperty("response", NullValueHandling = NullValueHandling.Ignore)]
    public new OllamaResponse.Analysis? Response { get; set; }
}
