using Telegram.Bot;
using Telegram.Bot.Types;
using TGBot.Settings;
using TGBot.Bot.Commands;
using TGBot.Text;

namespace TGBot.Bot;

public sealed class UpdateHandler
{
    private readonly CommandDispatcher _commandDispatcher;
    private readonly UserSessionStore _sessionStore;
    private readonly AiRequestHandler _aiRequestHandler;
    private readonly MessageSender _messageSender;
    private readonly KeyboardProvider _keyboards;
    private readonly BotSettings _settings;

    public UpdateHandler(
        CommandDispatcher commandDispatcher,
        UserSessionStore sessionStore,
        AiRequestHandler aiRequestHandler,
        MessageSender messageSender,
        KeyboardProvider keyboards,
        BotSettings settings)
    {
        _commandDispatcher = commandDispatcher;
        _sessionStore = sessionStore;
        _aiRequestHandler = aiRequestHandler;
        _messageSender = messageSender;
        _keyboards = keyboards;
        _settings = settings;
    }

    public async Task HandleAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
    {
        if (update.Message is not { } message || message.Text is null)
            return;

        long userId = message.From?.Id ?? 0;
        long chatId = message.Chat.Id;
        string text = message.Text;

        Console.WriteLine(message.Chat.Username);

        if (IsCommand(text))
        {
            IBotCommand? command = _commandDispatcher.Resolve(text);
            if (command is not null)
                await command.ExecuteAsync(bot, message, cancellationToken);
            return;
        }

        if (_sessionStore.TryTakeFeedback(userId))
        {
            await _messageSender.SendAsync(bot, chatId, BotText.FeedbackThanks, cancellationToken, _keyboards.MainMenu());
            await bot.SendMessage(_settings.CreatorChatId,
                BotText.FeedbackToCreator(message.Chat.Username, text),
                cancellationToken: cancellationToken);
        }

        IBotCommand? menuCommand = _commandDispatcher.Resolve(text);
        if (menuCommand is not null)
            await menuCommand.ExecuteAsync(bot, message, cancellationToken);

        if (_sessionStore.IsAiModeEnabled(userId))
            await _aiRequestHandler.HandleAsync(bot, message, cancellationToken);
    }

    private static bool IsCommand(string text) =>
        text.StartsWith('/') || text == BotText.AiModeButton;
}