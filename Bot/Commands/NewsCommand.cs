using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TGBot.Abstractions;
using TGBot.Text;

namespace TGBot.Bot.Commands;

public sealed class NewsCommand : IBotCommand
{
    private const int NewsCount = 5;

    private readonly INewsService _newsService;

    public NewsCommand(INewsService newsService)
    {
        _newsService = newsService;
    }

    public string Trigger => BotText.NewsButton;

    public bool Matches(string text) => text == Trigger;

    public async Task ExecuteAsync(ITelegramBotClient bot, Message message, CancellationToken cancellationToken)
    {
        IReadOnlyList<NewsItem> news = await _newsService.GetLatestAsync(NewsCount, cancellationToken);

        if (news.Count == 0)
        {
            await bot.SendMessage(message.Chat.Id, BotText.NoNews, cancellationToken: cancellationToken);
            return;
        }

        var keyboard = new InlineKeyboardMarkup(
            news.Select((item, index) =>
                new[]
                {
                    InlineKeyboardButton.WithUrl($"{index + 1}. {item.Title}", item.Url)
                }));

        await bot.SendMessage(message.Chat.Id, BotText.NewsHeader,
            cancellationToken: cancellationToken, replyMarkup: keyboard);
    }
}