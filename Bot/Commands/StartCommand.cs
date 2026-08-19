using Telegram.Bot;
using Telegram.Bot.Types;
using TGBot.Text;

namespace TGBot.Bot.Commands;

public sealed class StartCommand : IBotCommand
{
    private readonly MessageSender _messageSender;
    private readonly KeyboardProvider _keyboards;

    public StartCommand(MessageSender messageSender, KeyboardProvider keyboards)
    {
        _messageSender = messageSender;
        _keyboards = keyboards;
    }

    public string Trigger => "/start";

    public bool Matches(string text) => text == Trigger;

    public async Task ExecuteAsync(ITelegramBotClient bot, Message message, CancellationToken cancellationToken)
    {
        long chatId = message.Chat.Id;
        await bot.SendMessage(chatId, BotText.StartMessage, cancellationToken: cancellationToken);
        await _messageSender.SendAsync(bot, chatId, BotText.MainMenuPrompt, cancellationToken, _keyboards.MainMenu());
    }
}