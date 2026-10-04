namespace WebChatSignalR.Security;

// Validerar användarnamn på serversidan.
// Frontend-validering är bra för användarupplevelsen,
// men säkerhetsregler ska alltid kontrolleras på servern.
public static class UsernameValidator
{
    public const int MaxLength = 20;

    public static string? Validate(string? username)
    {
        username = username?.Trim();

        if (string.IsNullOrWhiteSpace(username))
        {
            return "Användarnamn krävs.";
        }

        if (username.Length > MaxLength)
        {
            return $"Användarnamnet får max vara {MaxLength} tecken.";
        }

        return null;
    }

    public static string Clean(string username)
    {
        return username.Trim();
    }
}