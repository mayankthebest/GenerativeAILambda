using System.Net.Http;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI.Embeddings;
using Microsoft.SemanticKernel.AI.TextCompletion;
using SK.Connectors.Llama.TextCompletion;
using SK.Connectors.Llama.TextEmbedding;

namespace SK.Connectors.Llama
{
    /// <summary>
    /// Provides extension methods for the <see cref="KernelBuilder"/> class to configure Llama connectors.
    /// </summary>
    public static class LlamaKernelBuilderExtensions
    {
        /// <summary>
        /// Registers an Llama text completion service with the specified configuration.
        /// </summary>
        /// <param name="builder">The <see cref="KernelBuilder"/> instance.</param>
        /// <param name="model">The name of the Llama model.</param>
        /// <param name="apiKey">The API key required for accessing the Llama service.</param>
        /// <param name="endpoint">The endpoint URL for the text completion service.</param>
        /// <param name="serviceId">A local identifier for the given AI service.</param>
        /// <param name="setAsDefault">Indicates whether the service should be the default for its type.</param>
        /// <param name="httpClient">The optional <see cref="HttpClient"/> to be used for making HTTP requests.
        /// If not provided, a default <see cref="HttpClient"/> instance will be used.</param>
        /// <returns>The modified <see cref="KernelBuilder"/> instance.</returns>
        public static KernelBuilder WithLlamaTextCompletionService(this KernelBuilder builder,
            string model,
            string? apiKey = null,
            string? endpoint = null,
            string? serviceId = null,
            bool setAsDefault = false,
            HttpClient? httpClient = null)
        {
            builder.WithAIService<ITextCompletion>(serviceId, (parameters) =>
                new LlamaTextCompletion(
                    model,
                    apiKey,
                    HttpClientProvider.GetHttpClient(parameters.Config, httpClient, parameters.Logger),
                    endpoint),
                    setAsDefault);

            return builder;
        }

        /// <summary>
        /// Registers an Llama text embedding generation service with the specified configuration.
        /// </summary>
        /// <param name="builder">The <see cref="KernelBuilder"/> instance.</param>
        /// <param name="model">The name of the Llama model.</param>
        /// <param name="endpoint">The endpoint for the text embedding generation service.</param>
        /// <param name="serviceId">A local identifier for the given AI service.</param>
        /// <param name="setAsDefault">Indicates whether the service should be the default for its type.</param>
        /// <returns>The <see cref="KernelBuilder"/> instance.</returns>
        public static KernelBuilder WithLlamaTextEmbeddingGenerationService(this KernelBuilder builder,
            string model,
            string apiKey,
            string endpoint,
            string? serviceId = null,
            bool setAsDefault = false)
        {
            builder.WithAIService<ITextEmbeddingGeneration>(serviceId, (parameters) =>
                new LlamaTextEmbeddingGeneration(
                    model,
                    HttpClientProvider.GetHttpClient(parameters.Config, httpClient: null, parameters.Logger),
                    apiKey,
                    endpoint),
                    setAsDefault);

            return builder;
        }

        /// <summary>
        /// Registers an Llama text embedding generation service with the specified configuration.
        /// </summary>
        /// <param name="builder">The <see cref="KernelBuilder"/> instance.</param>
        /// <param name="model">The name of the Llama model.</param>
        /// <param name="httpClient">The optional <see cref="HttpClient"/> instance used for making HTTP requests.</param>
        /// <param name="endpoint">The endpoint for the text embedding generation service.</param>
        /// <param name="serviceId">A local identifier for the given AI serviceю</param>
        /// <param name="setAsDefault">Indicates whether the service should be the default for its type.</param>
        /// <returns>The <see cref="KernelBuilder"/> instance.</returns>
        public static KernelBuilder WithLlamaTextEmbeddingGenerationService(this KernelBuilder builder,
            string model,
            HttpClient? httpClient = null,
            string? endpoint = null,
            string? serviceId = null,
            bool setAsDefault = false)
        {
            builder.WithAIService<ITextEmbeddingGeneration>(serviceId, (parameters) =>
                new LlamaTextEmbeddingGeneration(
                    model,
                    HttpClientProvider.GetHttpClient(parameters.Config, httpClient, parameters.Logger),
                    endpoint),
                    setAsDefault);

            return builder;
        }
    }
}