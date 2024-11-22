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

        [HubMethodName("SendMsg")]
        public async Task SendMsg(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMsg", user, message);
        }

        [HubMethodName("SendPrivateMsg")]
        public async Task SendPrivateMsg(string user, string connId, string message)
        {
            await Clients.User(user).SendAsync("SendPrivateMsg", message);
            // await Clients.Client(connId).SendAsync("SendPrivateMsg", user, connId, message);

        }

        [HubMethodName("SendMsgToGroup")]
        public async Task SendMsgToGroup(string gName, string user, string message)
        {
            await Clients.Group(gName).SendAsync("SendMsg", $"{Context.ConnectionId}-{user}: {message}");
        }

        [HubMethodName("AddToGroup")]
        public async Task AddToGroup(string gName, string user)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, gName);
            await Clients.Group(gName).SendAsync("SendMsg", $"{user} : has joined {gName}!");
        }

        [HubMethodName("RemoveFromGroup")]
        public async Task RemoveFromGroup(string groupName, string user)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("SendMsg", $"{user} has left the group {groupName}");
        }

    }
}
