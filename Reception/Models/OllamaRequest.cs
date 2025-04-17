using System.Text.Json;
using System.Collections.Generic;

namespace Reception.Models;

public class OllamaRequest
{
    // Name of the model to run (required)
    // [JsonProperty("model")]
    public string Model { get; set; } = null!;

    // Text prompt for generation (required)
    // [JsonProperty("prompt")]
    public string Prompt { get; set; } = null!;

    // Array of Base64-images as strings
    // [JsonProperty("images")]
    public string[]? Images { get; set; }

    // Optional: stream back partial results
    // [JsonProperty("stream", NullValueHandling = NullValueHandling.Ignore)]
    public bool? Stream { get; set; }

    // Optional: number of tokens to predict
    // [JsonProperty("num_predict", NullValueHandling = NullValueHandling.Ignore)]
    public int? NumPredict { get; set; }

    // Optional: top_k sampling parameter
    // [JsonProperty("top_k", NullValueHandling = NullValueHandling.Ignore)]
    public int? TopK { get; set; }

    // Optional: top_p sampling parameter
    // [JsonProperty("top_p", NullValueHandling = NullValueHandling.Ignore)]
    public float? TopP { get; set; }

    // Optional: temperature parameter for randomness
    // [JsonProperty("temperature", NullValueHandling = NullValueHandling.Ignore)]
    public float? Temperature { get; set; }

    // Optional: penalty to reduce repetition
    // [JsonProperty("repeat_penalty", NullValueHandling = NullValueHandling.Ignore)]
    public float? RepeatPenalty { get; set; }

    // Optional: a seed value for deterministic results
    // [JsonProperty("seed", NullValueHandling = NullValueHandling.Ignore)]
    public int? Seed { get; set; }

    // Optional: list of stop strings to control generation stopping
    // [JsonProperty("stop", NullValueHandling = NullValueHandling.Ignore)]
    public List<string> Stop { get; set; }

    // Optional: extra custom options as key-value pairs
    // [JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
    public Dictionary<string, object> Options { get; set; }
}
