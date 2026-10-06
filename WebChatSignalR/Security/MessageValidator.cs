using Microsoft.AspNetCore.SignalR;

namespace WebChatSignalR.Security;

public class MessageValidator
{
    private const int MaxLength = 500;

    public static string? Validate(string? message)
    {
        var normalized = message?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new HubException("Meddelandet får inte vara tomt.");
        }

        if (normalized.Length > MaxLength)
        {
            throw new HubException($"Meddelandet får max vara {MaxLength} tecken.");
        }

        return normalized;
    }
    public static string Clean(string message)
    {
        return message.Trim();
    }
}
