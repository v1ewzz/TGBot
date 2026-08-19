using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TGBot.Abstractions;
using TGBot.Text;

namespace TGBot.Bot;

public sealed class AiRequestHandler
{
    private readonly IChatCompletionService _chatCompletion;
    private readonly IRequestLimiter _requestLimiter;
    private readonly UserSessionStore _sessionStore;
    private readonly MessageSender _messageSender;
    private readonly KeyboardProvider _keyboards;

    public AiRequestHandler(
        IChatCompletionService chatCompletion,
        IRequestLimiter requestLimiter,
        UserSessionStore sessionStore,
        MessageSender messageSender,
        KeyboardProvider keyboards)
    {
        _chatCompletion = chatCompletion;
        _requestLimiter = requestLimiter;
        _sessionStore = sessionStore;
        _messageSender = messageSender;
        _keyboards = keyboards;
    }

    public async Task HandleAsync(ITelegramBotClient bot, Message message, CancellationToken cancellationToken)
    {
        long userId = message.From?.Id ?? 0;

        if (!await _requestLimiter.TryConsumeRequestAsync(userId, cancellationToken))
        {
            _sessionStore.DisableAiMode(userId);
            await _messageSender.SendAsync(bot, message.Chat.Id, BotText.RequestsExhausted,
                cancellationToken, _keyboards.MainMenu());
            return;
        }

        await bot.SendChatAction(message.Chat.Id, ChatAction.Typing,
            cancellationToken: cancellationToken);

        string response = await _chatCompletion.AskAsync(message.Text!, cancellationToken);
        response = MarkdownFormatter.ToTelegram(response);

        await _messageSender.SendLongAsync(bot, message.Chat.Id, response, cancellationToken);
    }
}