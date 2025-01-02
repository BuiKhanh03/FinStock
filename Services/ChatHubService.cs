using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace api.Hubs
{
    public class ChatHub : Hub
    {
        public async Task NewMessage(long username, string message) =>
       await Clients.All.SendAsync("messageReceived", username, message);

        // Để người dùng có thể tham gia một nhóm cụ thể, ví dụ nhóm chat
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("messageReceived", "System", $"{Context.ConnectionId} has joined the group.");
        }

        // Rời nhóm khi người dùng không còn tham gia
        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("messageReceived", "System", $"{Context.ConnectionId} has left the group.");
        }
    }
}