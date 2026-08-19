using Telegram.Bot;
using Telegram.Bot.Types;
using TGBot.Bot.Commands;

namespace TGBot.Bot;

public sealed class CommandDispatcher
{
    private readonly IReadOnlyList<IBotCommand> _commands;
    private readonly IBotCommand _unknownCommand;

    public CommandDispatcher(IReadOnlyList<IBotCommand> commands, IBotCommand unknownCommand)
    {
        _commands = commands;
        _unknownCommand = unknownCommand;
    }

    public IBotCommand? Resolve(string text)
    {
        IBotCommand? command = _commands.FirstOrDefault(c => c.Matches(text));
        return command ?? (text.StartsWith('/') ? _unknownCommand : null);
    }
}