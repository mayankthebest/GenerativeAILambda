using SK.Connectors.Llama.TextCompletion;
using SK.Connectors.Llama.TextEmbedding;
using Amazon.SageMakerRuntime;
using Amazon.SageMakerRuntime.Model;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace SageMakerProxy.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SageMakerController : ControllerBase
    {
        private ILogger<SageMakerController> logger;
        public SageMakerController(ILogger<SageMakerController> logger)
        {
            this.logger = logger;
        }

        [HttpPost]
        [Route("gpt2")]
        public async Task<IActionResult> CallSageMakerChatAsync([FromBody] TextCompletionRequest textCompletionRequest)
        {
            logger.LogInformation("Calling chatting model");
            logger.LogInformation(JsonSerializer.Serialize(textCompletionRequest));
            return await CallHuggingFaceModel("jumpstart-dft-meta-textgeneration-llama-2-7b-f", JsonSerializer.SerializeToUtf8Bytes(textCompletionRequest), "application/json");
        }

        [HttpPost]
        [Route("all-MiniLM-L6-v2")]
        public async Task<IActionResult> CallSageMakerEmbeddingAsync([FromBody] TextEmbeddingRequest textEmbeddingRequest)
        {
            logger.LogInformation("Calling text embedding model");
            logger.LogInformation(JsonSerializer.Serialize(textEmbeddingRequest));
            return await CallHuggingFaceModel("jumpstart-dft-hf-textembedding-all-minilm-l6-v2", JsonSerializer.SerializeToUtf8Bytes(textEmbeddingRequest), "application/json");
        }

        private async Task<IActionResult> CallHuggingFaceModel(string endpointName, byte[] body, string contentType)
        {
            AmazonSageMakerRuntimeClient awsSageMakerRuntimeClient = new AmazonSageMakerRuntimeClient();
            InvokeEndpointRequest request = new InvokeEndpointRequest();
            request.EndpointName = endpointName;
            request.ContentType = contentType;
            request.Body = new MemoryStream(body);
            request.CustomAttributes = "accept_eula=true";
            var response = await awsSageMakerRuntimeClient.InvokeEndpointAsync(request);
            var result = Encoding.UTF8.GetString(response.Body.ToArray());
            return Ok(result);
        }
    }
}
