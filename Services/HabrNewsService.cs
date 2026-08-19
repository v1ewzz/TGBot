using System.ServiceModel.Syndication;
using System.Xml;
using TGBot.Abstractions;

namespace TGBot.Services;

public sealed class HabrNewsService : INewsService
{
    private const string HabrRssUrl = "https://habr.com/ru/rss/all/";

    public async Task<IReadOnlyList<NewsItem>> GetLatestAsync(int count, CancellationToken cancellationToken = default)
    {
        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null
        };

        using var reader = XmlReader.Create(HabrRssUrl, settings);
        var feed = SyndicationFeed.Load(reader);

        return feed.Items
            .Where(item => item.Title?.Text is not null && item.Links.FirstOrDefault() is not null)
            .Take(count)
            .Select(item => new NewsItem(item.Title!.Text, item.Links.First().Uri.ToString()))
            .ToList();
    }
}