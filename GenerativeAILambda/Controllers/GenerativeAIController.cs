using GenerativeAILambda.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI;
using Microsoft.SemanticKernel.Skills.Core;
using Microsoft.SemanticKernel.Text;
using Polly;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace GenerativeAILambda.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenerativeAIController : ControllerBase
    {
        private ILogger<GenerativeAIController> _logger;
        private IConfiguration _configuration;

        public GenerativeAIController(ILogger<GenerativeAIController> logger, IConfiguration configuration)
        {
            this._logger = logger;
            this._configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> ChatAsync([FromServices] IKernel kernel, [FromQuery] string input)
        {
            _logger.LogInformation($"User asked: {input}");
            var memorySkill = new TextMemorySkill();
            kernel.ImportSkill(memorySkill);
            var context = kernel.CreateNewContext();
            context[TextMemorySkill.CollectionParam] = this._configuration["DocumentMemory:GlobalDocumentCollectionName"];
            context[TextMemorySkill.RelevanceParam] = "0.6";
            context["input"] = input;
            // Llama only supports 'system', 'user' and 'assistant' roles, starting with 'system', then 'user' and alternating (u/a/u/a/u...).
            var prompt = @"
        {{ '{{ system }}' }} You are a bot which can answer queries on Medicare.
        Consider only 'Medicare FAQ' data while answering questions. Don't talk about anything else.
        
        Medicare FAQ: {{recall $input}}
        {{ '{{ user }}' }} {{$input}}
        ";
            var userAsk = kernel.CreateSemanticFunction(prompt);
            var result = await userAsk.InvokeAsync(context);

            //var result = kernel.Memory.SearchAsync(this._options.GlobalDocumentCollectionName,input,1,0.8);

            return Ok(result.Result);
        }

        /// <summary>
        /// Service API for importing a document.
        /// </summary>
        [Route("importDocument")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ImportDocumentAsync([FromServices] IKernel kernel, IFormFile uploadedFile)
        {
            this._logger.LogInformation("Importing {0} document...", uploadedFile.FileName);
            var fileType = this.GetFileType(Path.GetFileName(uploadedFile.FileName));
            var fileContent = string.Empty;
            switch (fileType)
            {
                case SupportedFileType.Txt:
                    fileContent = await this.ReadTxtFileAsync(uploadedFile);
                    break;
                case SupportedFileType.Pdf:
                    fileContent = this.ReadPdfFile(uploadedFile);
                    break;
                default:
                    return this.BadRequest($"Unsupported file type: {fileType}");
            }

            this._logger.LogInformation("Importing document {0}", uploadedFile.FileName);

            // Parse document content to memory
            try
            {
                await this.ParseDocumentContentToMemoryAsync(kernel, fileContent, "chatmemory", uploadedFile.FileName);
            }
            catch (Exception ex) when (!ex.IsCriticalException())
            {
                return this.BadRequest(ex.Message);
            }

            return this.Ok();
        }

        private SupportedFileType GetFileType(string fileName)
        {
            string extension = Path.GetExtension(fileName);
            return extension switch
            {
                ".txt" => SupportedFileType.Txt,
                ".pdf" => SupportedFileType.Pdf,
                _ => throw new ArgumentOutOfRangeException($"Unsupported file type: {extension}"),
            };
        }

        /// <summary>
        /// Read the content of a text file.
        /// </summary>
        /// <param name="file">An IFormFile object.</param>
        /// <returns>A string of the content of the file.</returns>
        private async Task<string> ReadTxtFileAsync(IFormFile file)
        {
            using var streamReader = new StreamReader(file.OpenReadStream());
            return await streamReader.ReadToEndAsync();
        }

        /// <summary>
        /// Read the content of a PDF file, ignoring images.
        /// </summary>
        /// <param name="fileName">An IFormFile object.</param>
        /// <returns>A string of the content of the file.</returns>
        private string ReadPdfFile(IFormFile file)
        {
            var fileContent = string.Empty;

            using var pdfDocument = PdfDocument.Open(file.OpenReadStream());
            foreach (var page in pdfDocument.GetPages())
            {
                var text = ContentOrderTextExtractor.GetText(page);
                fileContent += text;
            }

            return fileContent;
        }

        private async Task ParseDocumentContentToMemoryAsync(IKernel kernel, string content, string memorySourceId, string docName)
        {
            var targetCollectionName = this._configuration["DocumentMemory:GlobalDocumentCollectionName"];

            // Split the document into lines of text and then combine them into paragraphs.
            // Note that this is only one of many strategies to chunk documents. Feel free to experiment with other strategies.
            var lines = TextChunker.SplitPlainTextLines(content, Convert.ToInt32(this._configuration["DocumentMemory:DocumentLineSplitMaxTokens"]));
            var paragraphs = TextChunker.SplitPlainTextParagraphs(lines, Convert.ToInt32(this._configuration["DocumentMemory:DocumentParagraphSplitMaxLines"]));
            var retryPolicy = Policy.Handle<AIException>().WaitAndRetryForeverAsync((_) => TimeSpan.FromSeconds(1), (ex, _) =>
            {
                this._logger.LogWarning("Retrying to save information...");
            });

            for (var i = 0; i < paragraphs.Count; i++)
            {
                await retryPolicy.ExecuteAsync(async () =>
                {
                    var paragraph = paragraphs[i];
                    await kernel.Memory.SaveInformationAsync(
                        collection: targetCollectionName,
                        text: paragraph,
                        id: $"{memorySourceId}-{i}",
                        description: $"Document: {docName}");
                });
                
            }

            this._logger.LogInformation(
                "Parsed {0} paragraphs from local file {1}",
                paragraphs.Count,
                docName
            );
        }
    }
}
