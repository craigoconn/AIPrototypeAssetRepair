using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

public class EmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;

    public EmbeddingService()
    {
        _httpClient = new HttpClient();
        _apiKey = "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";
        _model = "text-embedding-3-small";
    }

    public async Task<IReadOnlyList<float>> GetEmbeddingAsync(string input)
    {
        var requestBody = new
        {
            input = input,
            model = _model
        };

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        var response = await _httpClient.PostAsync("https://api.openai.com/v1/embeddings", content);
        if (!response.IsSuccessStatusCode)
            throw new Exception($"OpenAI API error: {response.StatusCode}");

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(json)
                                 .RootElement
                                 .GetProperty("data")[0]
                                 .GetProperty("embedding");

        var vector = result.EnumerateArray().Select(x => x.GetSingle()).ToList();
        return vector;
    }
}
