using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Cryptography.X509Certificates;

namespace WebChatSignalR.Hubs;

[Authorize]
public class ChatHub : Hub
{
    public async Task SendMessage(string message)
    {
        var username = Context.User?.Identity?.Name;

        if (string.IsNullOrWhiteSpace(username))
        {
            throw new HubException("Du måste vara inloggad.");
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new HubException("Meddelandet får inte vara tomt.");
        }

        await Clients.All.SendAsync("ReceiveMessage", username, message.Trim());
    }
}
