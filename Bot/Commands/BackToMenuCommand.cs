using Telegram.Bot;
using Telegram.Bot.Types;
using TGBot.Text;

namespace TGBot.Bot.Commands;

public sealed class BackToMenuCommand : IBotCommand
{
    private readonly UserSessionStore _sessionStore;
    private readonly MessageSender _messageSender;
    private readonly KeyboardProvider _keyboards;

    public BackToMenuCommand(UserSessionStore sessionStore, MessageSender messageSender, KeyboardProvider keyboards)
    {
        _sessionStore = sessionStore;
        _messageSender = messageSender;
        _keyboards = keyboards;
    }

    public string Trigger => BotText.BackToMenuButton;

    public bool Matches(string text) => text == Trigger;

    public Task ExecuteAsync(ITelegramBotClient bot, Message message, CancellationToken cancellationToken)
    {
        long chatId = message.Chat.Id;
        long userId = message.From?.Id ?? 0;

        _sessionStore.Reset(userId);
        return _messageSender.SendAsync(bot, chatId, BotText.MainMenuPrompt, cancellationToken, _keyboards.MainMenu());
    }
}