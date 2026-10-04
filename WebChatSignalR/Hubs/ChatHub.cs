using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using WebChatSignalR.Security;

namespace WebChatSignalR.Hubs;

// [Authorize] gör att bara inloggade användare kan använda hubben.
// Detta uppfyller kravet att någon form av användarnamn krävs för chatten.
[Authorize]
public class ChatHub : Hub
{
    private const int MaxMessageLength = 500;

    private readonly ChatRateLimiter _rateLimiter;

    public ChatHub(ChatRateLimiter rateLimiter)
    {
        _rateLimiter = rateLimiter;
    }

    // Körs automatiskt när en klient ansluter till SignalR-hubben.
    public override async Task OnConnectedAsync()
    {
        var username = GetUsername();

        await Clients.Caller.SendAsync(
            "ReceiveSystem",
            $"Du är ansluten som {username}."
        );

        await Clients.Others.SendAsync(
            "ReceiveSystem",
            $"{username} anslöt sig till chatten."
        );

        await base.OnConnectedAsync();
    }

    // Körs automatiskt när en klient kopplar från.
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var username = GetUsername();

        _rateLimiter.RemoveConnection(Context.ConnectionId);

        await Clients.Others.SendAsync(
            "ReceiveSystem",
            $"{username} lämnade chatten."
        );

        await base.OnDisconnectedAsync(exception);
    }

    // Frontend ska anropar connection.invoke("SendMessage", message)
    //
    // Klienten skickar inte användarnamnet själv.
    // Servern hämtar användarnamnet från den autentiserade användarens claims.
    public async Task SendMessage(string message)
    {
        var username = GetUsername();
        var cleanedMessage = message?.Trim();

        if (string.IsNullOrWhiteSpace(cleanedMessage))
        {
            throw new HubException("Meddelandet får inte vara tomt.");
        }

        if (cleanedMessage.Length > MaxMessageLength)
        {
            throw new HubException($"Meddelandet får max vara {MaxMessageLength} tecken.");
        }

        if (!_rateLimiter.IsAllowed(Context.ConnectionId))
        {
            throw new HubException(
                $"Du skickar för snabbt. Försök igen om {_rateLimiter.CooldownSeconds} sekunder."
            );
        }

        _rateLimiter.MarkMessageSent(Context.ConnectionId);

        // Clients.All skickar meddelandet till alla anslutna klienter.
        // Alla klienter som lyssnar på ReceiveMessage visar meddelandet i realtid.
        await Clients.All.SendAsync("ReceiveMessage", username, cleanedMessage);
    }

    private string GetUsername()
    {
        return Context.User?.Identity?.Name ?? "Okänd användare";
    }
}
