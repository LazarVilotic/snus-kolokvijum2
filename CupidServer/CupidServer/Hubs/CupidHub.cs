using Microsoft.AspNetCore.SignalR;

namespace CupidServer.Hubs
{
    public class CupidHub : Hub
    {
        public async Task RegisterUser(string username)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, username);
        }

        public async Task SendLetter(string toUsername, string fromUsername, string message, string phone)
        {
            await Clients.Group(toUsername).SendAsync("ReceiveLetter", fromUsername, message, phone);
        }
    }
}