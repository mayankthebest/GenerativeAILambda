using SK.Connectors.Llama;
using SK.Connectors.Llama.TextEmbedding;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Memory.Qdrant;
using Microsoft.SemanticKernel.Memory;

namespace GenerativeAILambda;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IKernel>(sp =>
        {
            return Kernel.Builder.WithLogger(sp.GetRequiredService<ILogger<IKernel>>())
                .WithMemory(sp.GetRequiredService<ISemanticTextMemory>())
                .WithLlamaTextCompletionService(Configuration["AIService:Models:Completion"], Configuration["AIService:Key"], "https://hm8jl00e15.execute-api.us-east-1.amazonaws.com/Prod/api/SageMaker")
                .WithLlamaTextEmbeddingGenerationService(Configuration["AIService:Models:Completion"], Configuration["AIService:Key"], "https://hm8jl00e15.execute-api.us-east-1.amazonaws.com/Prod/api/SageMaker")
                .Build();
        });

        services.AddSingleton<IMemoryStore>(sp =>
        {
            HttpClient httpClient = new(new HttpClientHandler { CheckCertificateRevocationList = true });
            if (!string.IsNullOrWhiteSpace(Configuration["MemoriesStore:Qdrant:Key"]))
            {
                httpClient.DefaultRequestHeaders.Add("api-key", Configuration["MemoriesStore:Qdrant:Key"]);
            }

            var endPointBuilder = new UriBuilder(Configuration["MemoriesStore:Qdrant:Host"]);
            endPointBuilder.Port = Convert.ToInt32(Configuration["MemoriesStore:Qdrant:Port"]);

            return new QdrantMemoryStore(
                httpClient: httpClient,
                Convert.ToInt32(Configuration["MemoriesStore:Qdrant:VectorSize"]),
                endPointBuilder.ToString(),
                logger: sp.GetRequiredService<ILogger<IQdrantVectorDbClient>>()
            );
        });
        services.AddScoped<ISemanticTextMemory>(sp => new SemanticTextMemory(
        sp.GetRequiredService<IMemoryStore>(),
        new LlamaTextEmbeddingGeneration(Configuration["AIService:Models:Embedding"], "https://hm8jl00e15.execute-api.us-east-1.amazonaws.com/Prod/api/SageMaker", Configuration["AIService:Key"])));

        services.AddSwaggerGen();
        services.AddControllers();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapGet("/", async context =>
            {
                await context.Response.WriteAsync("Welcome to running ASP.NET Core on AWS Lambda");
            });
        });
    }
}