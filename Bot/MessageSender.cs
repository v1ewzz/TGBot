using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TGBot.Text;

namespace TGBot.Bot;

public sealed class MessageSender
{
    private const int MaxMessageLength = 4000;

    private readonly ReplyKeyboardMarkup _backToMainMenu;

    public MessageSender(ReplyKeyboardMarkup backToMainMenu)
    {
        _backToMainMenu = backToMainMenu;
    }

    public Task SendAsync(ITelegramBotClient bot, long chatId, string text,
        CancellationToken cancellationToken, ReplyKeyboardMarkup? replyMarkup = null) =>
        bot.SendMessage(chatId, text, cancellationToken: cancellationToken, replyMarkup: replyMarkup);

    public Task SendLongAsync(ITelegramBotClient bot, long chatId, string text, CancellationToken cancellationToken)
    {
        IEnumerable<string> parts = SplitByLength(text, MaxMessageLength);
        return SendAllAsync(bot, chatId, parts, cancellationToken);
    }

    private async Task SendAllAsync(ITelegramBotClient bot, long chatId, IEnumerable<string> parts,
        CancellationToken cancellationToken)
    {
        foreach (string part in parts)
        {
            await bot.SendMessage(chatId, part,
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken,
                replyMarkup: _backToMainMenu);
        }
    }

    private static IEnumerable<string> SplitByLength(string text, int maxLength)
    {
        while (text.Length > maxLength)
        {
            int cut = text.LastIndexOf('\n', maxLength - 1);
            if (cut <= 0)
                cut = maxLength;

            yield return text[..cut].Trim();
            text = text[cut..].TrimStart('\n');
        }

        if (text.Length > 0)
            yield return text;
    }
}