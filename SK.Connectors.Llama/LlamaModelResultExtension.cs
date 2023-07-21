using SK.Connectors.Llama.TextCompletion;
using Microsoft.SemanticKernel.Orchestration;

namespace SK.Connectors.Llama;

public static class LlamaModelResultExtension
{
    /// <summary>
    /// Retrieves a typed <see cref="TextCompletionResponse"/> Llama result from PromptResult/>.
    /// </summary>
    /// <param name="resultBase">Current context</param>
    /// <returns>Llama result <see cref="TextCompletionResponse"/></returns>
    public static TextCompletionResponse GetLlamaResult(this ModelResult resultBase)
    {
        return resultBase.GetResult<TextCompletionResponse>();
    }
}
