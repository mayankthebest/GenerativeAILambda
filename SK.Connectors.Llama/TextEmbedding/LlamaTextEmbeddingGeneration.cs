using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.AI;
using Microsoft.SemanticKernel.AI.Embeddings;

namespace SK.Connectors.Llama.TextEmbedding
{

    /// <summary>
    /// Llama embedding generation service.
    /// </summary>
    public sealed class LlamaTextEmbeddingGeneration : ITextEmbeddingGeneration
    {
        private const string HttpUserAgent = "Microsoft-Semantic-Kernel";
        private readonly string? _apiKey;
        private readonly string _model;
        private readonly string? _endpoint;
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="LlamaTextEmbeddingGeneration"/> class.
        /// Using default <see cref="HttpClientHandler"/> implementation.
        /// </summary>
        /// <param name="endpoint">Endpoint for service API call.</param>
        /// <param name="model">Model to use for service API call.</param>
        public LlamaTextEmbeddingGeneration(Uri endpoint, string model, string apiKey)
        {
            this._endpoint = endpoint.AbsoluteUri;
            this._model = model;
            _apiKey = apiKey;
            this._httpClient = new HttpClient(NonDisposableHttpClientHandler.Instance, disposeHandler: false);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LlamaTextEmbeddingGeneration"/> class.
        /// </summary>
        /// <param name="model">Model to use for service API call.</param>
        /// <param name="endpoint">Endpoint for service API call.</param>
        public LlamaTextEmbeddingGeneration(string model, string endpoint, string apiKey)
        {
            this._model = model;
            this._endpoint = endpoint;
            _apiKey = apiKey;
            this._httpClient = new HttpClient(NonDisposableHttpClientHandler.Instance, disposeHandler: false);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LlamaTextEmbeddingGeneration"/> class.
        /// </summary>
        /// <param name="model">Model to use for service API call.</param>
        /// <param name="httpClient">The HttpClient used for making HTTP requests.</param>
        /// <param name="endpoint">Endpoint for service API call. If not specified, the base address of the HTTP client is used.</param>
        public LlamaTextEmbeddingGeneration(string model, HttpClient httpClient, string apiKey, string? endpoint = null)
        {
            this._model = model;
            this._endpoint = endpoint;
            this._httpClient = httpClient;
            _apiKey = apiKey;
            if (httpClient.BaseAddress == null && string.IsNullOrEmpty(endpoint))
            {
                throw new AIException(
                    AIException.ErrorCodes.InvalidConfiguration,
                    "The HttpClient BaseAddress and endpoint are both null or empty. Please ensure at least one is provided.");
            }
        }

        /// <inheritdoc/>
        public async Task<IList<Embedding<float>>> GenerateEmbeddingsAsync(IList<string> data, CancellationToken cancellationToken = default)
        {
            return await this.ExecuteEmbeddingRequestAsync(data, cancellationToken).ConfigureAwait(false);
        }

        #region private ================================================================================

        /// <summary>
        /// Performs HTTP request to given endpoint for embedding generation.
        /// </summary>
        /// <param name="data">Data to embed.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
        /// <returns>List of generated embeddings.</returns>
        /// <exception cref="AIException">Exception when backend didn't respond with generated embeddings.</exception>
        private async Task<IList<Embedding<float>>> ExecuteEmbeddingRequestAsync(IList<string> data, CancellationToken cancellationToken)
        {
            try
            {
                var embeddingRequest = new TextEmbeddingRequest
                {
                    Input = data
                };

                var serial = JsonSerializer.Serialize(embeddingRequest);

                using var httpRequestMessage = HttpRequest.CreatePostRequest(this.GetRequestUri(), embeddingRequest);

                httpRequestMessage.Headers.Add("User-Agent", HttpUserAgent);

                if (!string.IsNullOrEmpty(this._apiKey))
                {
                    httpRequestMessage.Headers.Add("x-api-key", this._apiKey);
                }

                var response = await this._httpClient.SendAsync(httpRequestMessage, cancellationToken).ConfigureAwait(false);
                var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                var embeddingResponse = JsonSerializer.Deserialize<TextEmbeddingResponse>(body);

                return embeddingResponse?.Embeddings?.Select(l => new Embedding<float>(l!, transferOwnership: true)).ToList()!;
            }
            catch (Exception e) when (e is not AIException && !e.IsCriticalException())
            {
                throw new AIException(
                    AIException.ErrorCodes.UnknownError,
                    $"Something went wrong: {e.Message}", e);
            }
        }

        /// <summary>
        /// Retrieves the request URI based on the provided endpoint and model information.
        /// </summary>
        /// <returns>
        /// A <see cref="Uri"/> object representing the request URI.
        /// </returns>
        private Uri GetRequestUri()
        {
            string? baseUrl = null;

            if (!string.IsNullOrEmpty(this._endpoint))
            {
                baseUrl = this._endpoint;
            }
            else if (this._httpClient.BaseAddress?.AbsoluteUri != null)
            {
                baseUrl = this._httpClient.BaseAddress!.AbsoluteUri;
            }
            else
            {
                throw new AIException(AIException.ErrorCodes.InvalidConfiguration, "No endpoint or HTTP client base address has been provided");
            }

            return new Uri($"{baseUrl!.TrimEnd('/')}/{this._model}");
        }

        #endregion
    }
}

namespace SK.Connectors.Llama
{
    internal static class ExceptionExtensions
    {
        /// <summary>
        /// Check if an exception is of a type that should not be caught by the kernel.
        /// </summary>
        /// <param name="ex">Exception.</param>
        /// <returns>True if <paramref name="ex"/> is a critical exception and should not be caught.</returns>
        internal static bool IsCriticalException(this Exception ex)
            => ex is OutOfMemoryException
                or ThreadAbortException
                or AccessViolationException
                or AppDomainUnloadedException
                or BadImageFormatException
                or CannotUnloadAppDomainException
                or InvalidProgramException
                or StackOverflowException;
    }
}