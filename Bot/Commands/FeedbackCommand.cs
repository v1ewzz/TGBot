using Telegram.Bot;
using Telegram.Bot.Types;
using TGBot.Text;

namespace TGBot.Bot.Commands;

public sealed class FeedbackCommand : IBotCommand
{
    private readonly UserSessionStore _sessionStore;
    private readonly MessageSender _messageSender;

    public FeedbackCommand(UserSessionStore sessionStore, MessageSender messageSender)
    {
        _sessionStore = sessionStore;
        _messageSender = messageSender;
    }

    public string Trigger => BotText.FeedbackButton;

    public bool Matches(string text) => text == Trigger;

    public Task ExecuteAsync(ITelegramBotClient bot, Message message, CancellationToken cancellationToken)
    {
        _sessionStore.EnableFeedbackMode(message.From?.Id ?? 0);
        return _messageSender.SendAsync(bot, message.Chat.Id, BotText.FeedbackPrompt, cancellationToken);
    }
}