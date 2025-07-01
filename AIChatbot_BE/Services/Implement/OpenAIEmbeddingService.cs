using Microsoft.Extensions.Options;
using Repositories.Models;
using Services.Interface;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public class OpenAIEmbeddingService : IEmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly string _flaskUrl;

    public OpenAIEmbeddingService(IOptions<OpenAIOptions> options) // tái dùng OpenAIOptions cho tiện
    {
        _flaskUrl = options.Value.FlaskUrl ?? "http://localhost:5000/embed"; // thêm FlaskUrl trong config

        _httpClient = new HttpClient();
    }

    public async Task<List<float>> GenerateEmbeddingAsync(string text)
    {
        var requestBody = new { text = text };
        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(_flaskUrl, content);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Flask server error: {(int)response.StatusCode} - {response.ReasonPhrase}\n{error}");
        }

        var responseJson = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseJson);
        var embeddingArray = doc.RootElement.GetProperty("embedding").EnumerateArray();

        return embeddingArray.Select(e => e.GetSingle()).ToList();
    }
}
