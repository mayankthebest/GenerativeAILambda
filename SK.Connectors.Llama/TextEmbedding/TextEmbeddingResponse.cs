using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SK.Connectors.Llama.TextEmbedding;

/// <summary>
/// HTTP Schema for embedding response.
/// </summary>
public sealed class TextEmbeddingResponse
{
    ///// <summary>
    ///// Model containing embedding.
    ///// </summary>
    //public sealed class EmbeddingVector
    //{
    //    //[JsonPropertyName("embedding")]
    //    public IList<float>? Embedding { get; set; }
    //}

    /// <summary>
    /// List of embeddings.
    /// </summary>
    [JsonPropertyName("embedding")]
    public IList<IList<float>>? Embeddings { get; set; }
}
