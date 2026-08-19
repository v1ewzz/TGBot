namespace TGBot.Abstractions;

public interface IRequestLimiter
{
    Task EnsureInitializedAsync(CancellationToken cancellationToken = default);
    Task<bool> CanMakeRequestAsync(long userId, CancellationToken cancellationToken = default);
    Task<bool> TryConsumeRequestAsync(long userId, CancellationToken cancellationToken = default);
}