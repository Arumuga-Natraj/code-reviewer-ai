using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using CodeReviewer.Backend.Models;

namespace CodeReviewer.Backend.Services;

public class OllamaService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public OllamaService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<ReviewResponse> GenerateReviewAsync(string prompt)
    {
        var provider = _configuration["LLM:Provider"]?.ToLower() ?? "gemini"; // fallback to gemini

        if (provider == "ollama")
        {
            return await CallOllamaEndpointAsync(prompt);
        }
        else
        {
            return await CallGeminiEndpointAsync(prompt);
        }
    }

    private async Task<ReviewResponse> CallOllamaEndpointAsync(string prompt)
    {
        var endpoint = _configuration["Ollama:Endpoint"] ?? "http://localhost:11434/api/generate";
        var model = _configuration["Ollama:Model"] ?? "codellama";

        var payload = new
        {
            model = model,
            prompt = prompt,
            stream = false,
            format = "json" // Instruct Ollama to output valid JSON
        };

        var requestContent = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _httpClient.PostAsync(endpoint, requestContent);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Ollama service error: {response.ReasonPhrase}. Details: {error}");
        }

        var responseBody = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseBody);
        var jsonText = doc.RootElement.GetProperty("response").GetString();

        if (string.IsNullOrWhiteSpace(jsonText))
        {
            throw new Exception("Ollama returned an empty response.");
        }

        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        return JsonSerializer.Deserialize<ReviewResponse>(jsonText, jsonOptions) ?? new ReviewResponse();
    }

    private async Task<ReviewResponse> CallGeminiEndpointAsync(string prompt)
    {
        var apiKey = _configuration["Gemini:ApiKey"] ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new Exception("Gemini API key is not configured in appsettings.json or environment variables.");
        }

        var model = _configuration["Gemini:Model"] ?? "gemini-1.5-flash";
        var geminiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

        var geminiPayload = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            },
            generationConfig = new
            {
                responseMimeType = "application/json",
                responseSchema = new
                {
                    type = "OBJECT",
                    properties = new
                    {
                        reviews = new
                        {
                            type = "ARRAY",
                            items = new
                            {
                                type = "OBJECT",
                                properties = new
                                {
                                    filePath = new { type = "STRING" },
                                    originalCode = new { type = "STRING" },
                                    suggestedCode = new { type = "STRING" },
                                    comment = new { type = "STRING" },
                                    lineStart = new { type = "INTEGER" },
                                    lineEnd = new { type = "INTEGER" }
                                },
                                required = new[] { "filePath", "originalCode", "suggestedCode", "comment" }
                            }
                        }
                    }
                }
            }
        };

        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var requestContent = new StringContent(JsonSerializer.Serialize(geminiPayload, jsonOptions), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(geminiUrl, requestContent);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Gemini API Error: {response.ReasonPhrase}. Details: {error}");
        }

        var responseBody = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseBody);
        
        var textResult = doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        if (string.IsNullOrWhiteSpace(textResult))
        {
            throw new Exception("Gemini returned an empty response content.");
        }

        return JsonSerializer.Deserialize<ReviewResponse>(textResult, jsonOptions) ?? new ReviewResponse();
    }
}
