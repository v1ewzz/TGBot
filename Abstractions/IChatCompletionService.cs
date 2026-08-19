namespace TGBot.Abstractions;

public interface IChatCompletionService
{
    Task<string> AskAsync(string prompt, CancellationToken cancellationToken = default);
}