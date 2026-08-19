using System.Collections.Concurrent;

namespace TGBot.Bot;

public sealed class UserSessionStore
{
    private readonly ConcurrentDictionary<long, bool> _aiModeByUser = new();
    private readonly ConcurrentDictionary<long, bool> _feedbackModeByUser = new();

    public void EnableAiMode(long userId) => _aiModeByUser[userId] = true;

    public void DisableAiMode(long userId) => _aiModeByUser.TryRemove(userId, out _);

    public bool IsAiModeEnabled(long userId) => _aiModeByUser.TryGetValue(userId, out bool enabled) && enabled;

    public void EnableFeedbackMode(long userId) => _feedbackModeByUser[userId] = true;

    public bool TryTakeFeedback(long userId) => _feedbackModeByUser.TryRemove(userId, out _);

    public void Reset(long userId)
    {
        DisableAiMode(userId);
        _feedbackModeByUser.TryRemove(userId, out _);
    }
}