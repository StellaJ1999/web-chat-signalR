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
            return "Meddelandet får inte vara tomt.";
        }

        if (normalized.Length > MaxLength)
        {
            return $"Meddelandet får max vara {MaxLength} tecken.";
        }

        return null;
    }
    public static string Clean(string message)
    {
        return message.Trim();
    }
}
