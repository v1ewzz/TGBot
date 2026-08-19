namespace TGBot.Text;

public static class BotText
{
    public const string AiModeButton = "ИИ-помощник";
    public const string NewsButton = "Новости";
    public const string AboutButton = "О боте";
    public const string FeedbackButton = "Оставить сообщение создателю";
    public const string BackToMenuButton = "⬅️Вернуться в меню";

    public const string AiModeEnabled = "Включен режим ИИ-помощника!";
    public const string RequestsExhausted =
        "У вас закончились запросы! Чтобы продолжить пользоваться этим режимом - пополните баланс!";
    public const string MainMenuPrompt = "Выберите пункт меню:";
    public const string WrongCommand = "Введена неправильная команда!";
    public const string FeedbackThanks = "Спасибо за ваш отзыв!";
    public const string FeedbackPrompt = "Напишите свое сообщение ниже:";
    public const string NewsHeader = "✅Источник - Habr\n\n📰Последние новости:";
    public const string NoNews = "Новостей пока нет, попробуйте позже.";

    private static readonly Lazy<string> StartMessageLazy = new(() => ReadTextOr("StartMessage.txt", "Добро пожаловать!"));
    private static readonly Lazy<string> AboutMessageLazy = new(() => ReadTextOr("AboutBotMessage.txt", "О боте:"));

    public static string StartMessage => StartMessageLazy.Value;
    public static string AboutMessage => AboutMessageLazy.Value;

    public static string FeedbackToCreator(string? username, string text) =>
        $"Вам пришло сообщение от @{username ?? "unknown"}:\n\n{text}";

    private static string ReadTextOr(string fileName, string fallback)
    {
        string path = Path.Combine(AppContext.BaseDirectory, fileName);
        return File.Exists(path) ? File.ReadAllText(path) : fallback;
    }
}