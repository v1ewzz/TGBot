using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using TGBot;
internal class PerplexityService
{
    private readonly string PERPLEXITY_API_KEY;
    private readonly HttpClient httpClient;
    private static ReplyKeyboardMarkup backToMainMenuButton = BotKeyboard.BackToMainMenu();

    internal PerplexityService(string apiKey, HttpClient client)
    {
        PERPLEXITY_API_KEY = apiKey;
        httpClient = client;
    }
    internal async Task<string> QueryPerplexityChat(string userText)
    {
        var requestBody = new
        {
            model = "sonar",
            messages = new[]
            {
                new { role = "user", content = userText }
            },
            return_images = false,
            return_related_questions = true,
            temperature = 0.8,
            web_search_options = new {
                search_context_size = "low",
                image_search_relevance_enhanced = false
            }
    };

        string json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.perplexity.ai/chat/completions")
        {
            Content = content
        };

        request.Headers.Add("Authorization", "Bearer " + PERPLEXITY_API_KEY);
        var response = await httpClient.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();
        Console.WriteLine(responseString);

        if (!response.IsSuccessStatusCode)
        {
            return $"ошибка при запросе к Perplexity: {response.StatusCode}\n{responseString}";
        }

        try
        {
            using var doc = JsonDocument.Parse(responseString);
            var msg = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();
            return msg ?? "No answer";
        }
        catch
        {
            return $"Ошибка разбора:\n{responseString}";
        }
    }

    internal async Task SendLongMessage(ITelegramBotClient bot, long chatId, string text, CancellationToken token)
    {
        const int maxLength = 4000; // для запаса
        for (int i = 0; i < text.Length; i += maxLength)
        {
            string part = text.Substring(i, Math.Min(maxLength, text.Length - i));
            await bot.SendMessage(chatId, part, parseMode: Telegram.Bot.Types.Enums.ParseMode.MarkdownV2, cancellationToken: token, replyMarkup: backToMainMenuButton);
        }
    }
}
