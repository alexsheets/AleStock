using Microsoft.AspNetCore.SignalR;
using AleStock.Models;

namespace AleStock.Hubs
{
    public class ChatHub : Hub
    {

        // task created for joining chatroom
        public async Task JoinChat(UserConnection conn)
        {
            await Clients.All.SendAsync("ReceiveMsg", "admin", $"{conn.User} has joined! Please be kind to them.");
        }

        public async Task SendMsg(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMsg", user, message);
        }

    }
}
