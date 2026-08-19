using Telegram.Bot;
using Telegram.Bot.Types;
using TGBot.Abstractions;
using TGBot.Text;

namespace TGBot.Bot.Commands;

public sealed class AiModeCommand : IBotCommand
{
    private readonly IRequestLimiter _requestLimiter;
    private readonly UserSessionStore _sessionStore;
    private readonly MessageSender _messageSender;
    private readonly KeyboardProvider _keyboards;

    public AiModeCommand(
        IRequestLimiter requestLimiter,
        UserSessionStore sessionStore,
        MessageSender messageSender,
        KeyboardProvider keyboards)
    {
        _requestLimiter = requestLimiter;
        _sessionStore = sessionStore;
        _messageSender = messageSender;
        _keyboards = keyboards;
    }

    public string Trigger => BotText.AiModeButton;

    public bool Matches(string text) => text is "/ai" or "/perplexity" or BotText.AiModeButton;

    public async Task ExecuteAsync(ITelegramBotClient bot, Message message, CancellationToken cancellationToken)
    {
        long chatId = message.Chat.Id;
        long userId = message.From?.Id ?? 0;

        if (!await _requestLimiter.CanMakeRequestAsync(userId, cancellationToken))
        {
            await _messageSender.SendAsync(bot, chatId, BotText.RequestsExhausted, cancellationToken);
            await _messageSender.SendAsync(bot, chatId, BotText.MainMenuPrompt, cancellationToken, _keyboards.MainMenu());
            return;
        }

        _sessionStore.EnableAiMode(userId);
        await _messageSender.SendAsync(bot, chatId, BotText.AiModeEnabled, cancellationToken, _keyboards.BackToMainMenu());
    }
}