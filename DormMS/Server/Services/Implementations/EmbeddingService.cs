using OllamaSharp;
using OllamaSharp.Models;
using DormMS.Server.Services.Interfaces;

namespace DormMS.Server.Services.Implementations;

public class EmbeddingService : IEmbeddingService
{
    private readonly OllamaApiClient _ollama;

    public EmbeddingService(IConfiguration config)
    {
        var uri = new Uri(config["Ollama:BaseUrl"] ?? "http://localhost:11434");
        _ollama = new OllamaApiClient(uri, "qwen3-embedding:0.6b");
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text)
    {
        var request = new EmbedRequest
        {
            Model = "qwen3-embedding:0.6b",
            Input = new List<string> { text }
        };

        var response = await _ollama.EmbedAsync(request);
        return response.Embeddings.First();
    }
}