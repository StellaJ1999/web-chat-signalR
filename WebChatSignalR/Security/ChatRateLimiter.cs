using System.Collections.Concurrent;

namespace WebChatSignalR.Security;

public sealed class ChatRateLimiter
{
    private readonly ConcurrentDictionary<string, DateTimeOffset> _lastMessageAt = new();

    public int CooldownSeconds { get; } = 2;

    public bool IsAllowed(string connectionId)
    {
        if (!_lastMessageAt.TryGetValue(connectionId, out var lastMessageAt))
        {
            return true;
        }

        return DateTimeOffset.UtcNow - lastMessageAt >= TimeSpan.FromSeconds(CooldownSeconds);
    }

    public void MarkMessageSent(string connectionId)
    {
        _lastMessageAt[connectionId] = DateTimeOffset.UtcNow;
    }

    public void RemoveConnection(string connectionId)
    {
        _lastMessageAt.TryRemove(connectionId, out _);
    }
}