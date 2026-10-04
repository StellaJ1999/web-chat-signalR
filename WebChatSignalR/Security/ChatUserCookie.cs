using System.Security.Claims;

namespace WebChatSignalR.Security;

// Samlar auth scheme, cookie-namn och skapandet av användarens claims.
// Detta gör att samma namn används i Program.cs, login och logout.
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
            //Inte nödvändigt för G-krav men finns för potentiell utökning av claims.
            new(ClaimTypes.Role, "User")
        };

        var identity = new ClaimsIdentity(claims, AuthScheme);

        return new ClaimsPrincipal(identity);
    }
}