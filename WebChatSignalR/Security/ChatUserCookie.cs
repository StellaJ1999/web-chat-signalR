using System.Security.Claims;

namespace WebChatSignalR.Security;

public static class ChatUserCookie
{
    public const string AuthScheme = "ChatCookie";
    public const string CookieName = "chat_auth";

    public static ClaimsPrincipal CreatePrincipal(string username)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, username),
            new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new(ClaimTypes.Role, "User")
        };

        var identity = new ClaimsIdentity(claims, AuthScheme);

        return new ClaimsPrincipal(identity);
    }
}