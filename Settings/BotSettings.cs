namespace TGBot.Settings;

public sealed record BotSettings(
    string TelegramBotApiKey,
    string GroqApiKey,
    string DbConnectionString,
    long CreatorChatId)
{
    private const string DefaultConnectionString =
        "Data Source=tgbot.db";
    private const long DefaultCreatorChatId = 0;

    public static BotSettings FromEnvironment()
    {
        return new BotSettings(
            Require("TELEGRAM_BOT_API_KEY"),
            Require("GROQ_API_KEY"),
            GetOr("DB_CONNECTION_STRING", DefaultConnectionString),
            GetLongOr("CREATOR_CHAT_ID", DefaultCreatorChatId));
    }

    private static string Require(string name) =>
        Get(name) ?? throw new InvalidOperationException($"Не задана переменная окружения {name}");

    private static string GetOr(string name, string fallback) => Get(name) ?? fallback;

    private static long GetLongOr(string name, long fallback) =>
        long.TryParse(Get(name), out long value) ? value : fallback;

    private static string? Get(string name) => Environment.GetEnvironmentVariable(name);
}