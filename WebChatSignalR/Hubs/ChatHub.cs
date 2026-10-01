using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace WebChatSignalR.Hubs;

public class ChatHub : Hub
{
    private const int MaxMessageLength = 500;

    // Anropas automatiskt när en klient ansluter till hubben.
    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync(
            "ReceiveSystem",
            "Du är ansluten till chatten."
        );

        await Clients.Others.SendAsync(
            "ReceiveSystem",
            "En användare anslöt till chatten."
        );

        await base.OnConnectedAsync();
    }

    // Anropas automatiskt när en klient kopplar från.
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Clients.Others.SendAsync(
            "ReceiveSystem",
            "En användare lämnade chatten."
        );

        await base.OnDisconnectedAsync(exception);
    }

    // Den här metoden anropas från klienten, t.ex:
    // connection.invoke("SendMessage", username, message)
    public async Task SendMessage(string username, string message)
    {
        username = username?.Trim() ?? "";
        message = message?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(username))
        {
            throw new HubException("Användarnamn krävs.");
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new HubException("Meddelandet får inte vara tomt.");
        }

        if (message.Length > MaxMessageLength)
        {
            throw new HubException($"Meddelandet får max vara {MaxMessageLength} tecken.");
        }

        // Skickar meddelandet till alla anslutna klienter.
        // Alla klienter måste lyssna på eventet "ReceiveMessage".
        await Clients.All.SendAsync("ReceiveMessage", username, message);
    }
}
