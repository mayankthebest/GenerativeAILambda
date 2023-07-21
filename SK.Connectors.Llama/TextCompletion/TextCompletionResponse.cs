using System.Text.Json.Serialization;

namespace SK.Connectors.Llama.TextCompletion;

/// <summary>
/// HTTP Schema for completion response.
/// </summary>
public sealed class TextCompletionResponse
{
    /// <summary>
    /// Completed text.
    /// </summary>
    [JsonPropertyName("generation")]
    public LlamaGeneration Generation { get; set; }

    public class LlamaGeneration
    {
        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }
    }
}
