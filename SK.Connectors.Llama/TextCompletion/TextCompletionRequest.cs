// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SK.Connectors.Llama.TextCompletion;

/// <summary>
/// HTTP schema to perform completion request.
/// </summary>
[Serializable]
public sealed class TextCompletionRequest
{
    /// <summary>
    /// Prompt to complete.
    /// </summary>
    [JsonPropertyName("inputs")]
    public List<List<Chat>> Inputs { get; set; }

    [JsonPropertyName("parameters")]
    public AIParameters Parameters { get; set; }

    public sealed class AIParameters
    {
        [JsonPropertyName("max_new_tokens")]
        public int MaxNewTokens { get; set; }

        [JsonPropertyName("top_p")]
        public double TopP { get; set; }

        [JsonPropertyName("temperature")]
        public double Temperature { get; set; }
    }

    public sealed class Chat
    {
        [JsonPropertyName("role")]
        public string Role { get;  set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }
    }
}