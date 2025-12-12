using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Collections.Concurrent;

namespace TGBot
{
    public class NewsService
    {
        private const string HABR_RSS = "https://habr.com/ru/rss/all/";

        public async Task ShowNews(ITelegramBotClient bot, long chatId, CancellationToken ct)
        {
            try
            {
                using var reader = XmlReader.Create(HABR_RSS);
                var feed = SyndicationFeed.Load(reader);

                var news = feed.Items.Take(3)
                    .Select(item => (Title: item.Title!.Text, Url: item.Links.First().Uri.ToString()))
                    .ToList();

                InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup(
                    news.Select((n, i) =>
                        new[]
                        {
                            InlineKeyboardButton.WithUrl($"{i + 1}. {n.Title}", n.Url)
                        }
                    ).ToArray());

                await bot.SendMessage(chatId,
                    $"✅Источник - Habr\n\n" +
                    $"📰Последние новости:",
                    cancellationToken: ct, replyMarkup: keyboard);
            }
            catch (Exception ex)
            {
                await bot.SendMessage(chatId, $"ошибка: {ex.Message}", cancellationToken: ct);
            }
        }
    }

}
