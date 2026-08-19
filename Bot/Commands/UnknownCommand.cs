using Telegram.Bot;
using Telegram.Bot.Types;
using TGBot.Text;

namespace TGBot.Bot.Commands;

public sealed class UnknownCommand : IBotCommand
{
    private readonly MessageSender _messageSender;

    public UnknownCommand(MessageSender messageSender)
    {
        _messageSender = messageSender;
    }

    public string Trigger => string.Empty;

    public bool Matches(string text) => false;

    public Task ExecuteAsync(ITelegramBotClient bot, Message message, CancellationToken cancellationToken) =>
        _messageSender.SendAsync(bot, message.Chat.Id, BotText.WrongCommand, cancellationToken);
}