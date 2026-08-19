using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TGBot.Abstractions;
using TGBot.Bot;
using TGBot.Bot.Commands;
using TGBot.Services;
using TGBot.Settings;

namespace TGBot;

internal static class Program
{
    private static async Task Main()
    {
        EnvLoader.Load();
        BotSettings settings = BotSettings.FromEnvironment();

        IRequestLimiter requestLimiter = new UserRequestsService(settings.DbConnectionString);
        await requestLimiter.EnsureInitializedAsync();

        using HttpClient httpClient = new();
        IChatCompletionService chatCompletion = new GroqChatService(settings.GroqApiKey, httpClient);
        INewsService newsService = new HabrNewsService();

        KeyboardProvider keyboards = new();
        MessageSender messageSender = new(keyboards.BackToMainMenu());
        UserSessionStore sessionStore = new();
        AiRequestHandler aiRequestHandler = new(chatCompletion, requestLimiter, sessionStore, messageSender, keyboards);

        CommandDispatcher commandDispatcher = new(
            [
                new StartCommand(messageSender, keyboards),
                new AiModeCommand(requestLimiter, sessionStore, messageSender, keyboards),
                new BackToMenuCommand(sessionStore, messageSender, keyboards),
                new AboutCommand(messageSender, keyboards),
                new FeedbackCommand(sessionStore, messageSender),
                new NewsCommand(newsService)
            ],
            new UnknownCommand(messageSender));

        UpdateHandler updateHandler = new(commandDispatcher, sessionStore, aiRequestHandler, messageSender, keyboards, settings);

        var bot = new TelegramBotClient(settings.TelegramBotApiKey);
        using var cts = new CancellationTokenSource();

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = [UpdateType.Message],
            DropPendingUpdates = false
        };

        bot.StartReceiving(updateHandler.HandleAsync, HandleError, receiverOptions, cts.Token);

        User me = await bot.GetMe();
        Console.WriteLine($"Бот {me.FirstName} (@{me.Username}) запущен");

        Console.ReadLine();
        cts.Cancel();
    }

    private static Task HandleError(ITelegramBotClient bot, Exception error, CancellationToken cancellationToken)
    {
        string message = error is Telegram.Bot.Exceptions.ApiRequestException apiEx
            ? $"Telegram API Error: [{apiEx.ErrorCode}] {apiEx.Message}"
            : error.ToString();
        Console.WriteLine(message);
        return Task.CompletedTask;
    }
}