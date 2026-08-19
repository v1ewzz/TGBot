using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using TGBot.Abstractions;

namespace TGBot.Services;

public sealed class GroqChatService : IChatCompletionService
{
    private const string Endpoint = "https://api.groq.com/openai/v1/chat/completions";
    private const string Model = "openai/gpt-oss-120b";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly string _apiKey;
    private readonly HttpClient _httpClient;

    public GroqChatService(string apiKey, HttpClient httpClient)
    {
        _apiKey = apiKey;
        _httpClient = httpClient;
    }

    public async Task<string> AskAsync(string prompt, CancellationToken cancellationToken = default)
    {
        var requestBody = new ChatCompletionRequest
        {
            Model = Model,
            Messages =
            [
                new ChatMessage { Role = "user", Content = prompt }
            ],
            Temperature = 1,
            MaxCompletionTokens = 2048,
            TopP = 1,
            Stream = false,
            ReasoningEffort = "medium"
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, Endpoint)
        {
            Content = JsonContent.Create(requestBody, options: JsonOptions)
        };
        request.Headers.Authorization = new("Bearer", _apiKey);

        HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
        string responseString = await response.Content.ReadAsStringAsync(cancellationToken);
        Console.WriteLine(responseString);

        if (!response.IsSuccessStatusCode)
            return $"ошибка при запросе к Groq: {response.StatusCode}\n{responseString}";

        try
        {
            using var doc = JsonDocument.Parse(responseString);
            string? content = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();
            return content ?? "No answer";
        }
        catch
        {
            return $"Ошибка разбора:\n{responseString}";
        }
    }

    private sealed class ChatCompletionRequest
    {
        public string Model { get; init; } = null!;
        public IReadOnlyList<ChatMessage> Messages { get; init; } = [];
        public int MaxCompletionTokens { get; init; }
        public double Temperature { get; init; }
        public double TopP { get; init; }
        public bool Stream { get; init; }
        public string ReasoningEffort { get; init; } = null!;
    }

    private sealed class ChatMessage
    {
        public string Role { get; init; } = null!;
        public string Content { get; init; } = null!;
    }
}