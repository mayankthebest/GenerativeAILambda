using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.AI.TextCompletion;
using Microsoft.SemanticKernel.Orchestration;

namespace SK.Connectors.Llama.TextCompletion;

internal sealed class TextCompletionStreamingResult : ITextStreamingResult
{
    private readonly ModelResult _responseData;

    public TextCompletionStreamingResult(TextCompletionResponse responseData)
    {
        this._responseData = new ModelResult(responseData);
    }

    public ModelResult ModelResult => this._responseData;

    public Task<string> GetCompletionAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(this._responseData.GetResult<TextCompletionResponse>().Generation.Content ?? string.Empty);
    }

    public async IAsyncEnumerable<string> GetCompletionStreamingAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        yield return await this.GetCompletionAsync(cancellationToken).ConfigureAwait(false);
    }
}
