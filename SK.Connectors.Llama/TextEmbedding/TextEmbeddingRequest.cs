using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SK.Connectors.Llama.TextEmbedding;

/// <summary>
/// HTTP schema to perform embedding request.
/// </summary>
[Serializable]
public sealed class TextEmbeddingRequest
{
    /// <summary>
    /// Data to embed.
    /// </summary>
    [JsonPropertyName("text_inputs")]
    public IList<string> Input { get; set; } = new List<string>();
}
