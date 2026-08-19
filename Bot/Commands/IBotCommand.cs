using Telegram.Bot;
using Telegram.Bot.Types;

namespace TGBot.Bot.Commands;

public interface IBotCommand
{
    string Trigger { get; }
    bool Matches(string text);
    Task ExecuteAsync(ITelegramBotClient bot, Message message, CancellationToken cancellationToken);
}