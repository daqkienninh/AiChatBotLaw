using Microsoft.Extensions.Options;
using Repositories.Models;
using Services.Interface;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public class OpenAIEmbeddingService : IEmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly int _maxRetries = 3;

    public OpenAIEmbeddingService(IOptions<OpenAIOptions> options)
    {
        _apiKey = options.Value.ApiKey ?? throw new Exception("Missing OpenAI API key");
        _model = options.Value.EmbeddingModel ?? "text-embedding-ada-002";

        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _apiKey);
    }

    public async Task<List<float>> GenerateEmbeddingAsync(string text)
    {
        var requestBody = new
        {
            input = text,
            model = _model
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        int retry = 0;

        while (retry < _maxRetries)
        {
            var response = await _httpClient.PostAsync(
                "https://api.openai.com/v1/embeddings", content);

            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(body);

                return doc.RootElement.GetProperty("data")[0]
                    .GetProperty("embedding")
                    .EnumerateArray()
                    .Select(e => e.GetSingle())
                    .ToList();
            }
            else if ((int)response.StatusCode == 429)
            {
                // Quá giới hạn, đợi và thử lại
                retry++;
                Console.WriteLine($"Rate limited (429). Retrying {retry}/{_maxRetries}...");
                await Task.Delay(2000 * retry);
            }
            else
            {
                // Các lỗi khác (401, 400, 500, ...)
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"OpenAI API error: {(int)response.StatusCode} - {response.ReasonPhrase}\n{errorContent}");
            }
        }

        throw new Exception("Failed to generate embedding after multiple attempts.");
    }
}
