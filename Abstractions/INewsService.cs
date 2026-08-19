namespace TGBot.Abstractions;

public sealed record NewsItem(string Title, string Url);

public interface INewsService
{
    Task<IReadOnlyList<NewsItem>> GetLatestAsync(int count, CancellationToken cancellationToken = default);
}