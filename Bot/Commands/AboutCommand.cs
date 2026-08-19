using Telegram.Bot;
using Telegram.Bot.Types;
using TGBot.Text;

namespace TGBot.Bot.Commands;

public sealed class AboutCommand : IBotCommand
{
    private readonly MessageSender _messageSender;
    private readonly KeyboardProvider _keyboards;

    public AboutCommand(MessageSender messageSender, KeyboardProvider keyboards)
    {
        _messageSender = messageSender;
        _keyboards = keyboards;
    }

    public string Trigger => BotText.AboutButton;

    public bool Matches(string text) => text == Trigger;

    public Task ExecuteAsync(ITelegramBotClient bot, Message message, CancellationToken cancellationToken) =>
        _messageSender.SendAsync(bot, message.Chat.Id, BotText.AboutMessage, cancellationToken, _keyboards.MessageToCreatorMenu());
}