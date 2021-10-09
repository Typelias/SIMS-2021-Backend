using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

namespace NewWebRTCBackend.Hubs
{
    public class SignalRTCHub : Hub
    {
        private IDictionary<string, List<string>> users;
        private IDictionary<string, string> socketToRoom;

        public SignalRTCHub(IDictionary<string, List<string>> users, IDictionary<string, string> userToRoom)
        {
            this.users = users;
            this.socketToRoom = userToRoom;
        }

        public async Task JoinRoom(string roomID)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomID);

            if (users.ContainsKey(roomID))
            {
                users[roomID].Add(Context.ConnectionId);
            }
            else
            {
                users.Add(roomID, new List<string>());
                users[roomID].Add(Context.ConnectionId);
            }

            this.socketToRoom.Add(Context.ConnectionId, roomID);

            var sendData = JsonSerializer.Serialize(users[roomID]);
            await Clients.Client(Context.ConnectionId).SendAsync("AllUsers", sendData);
            await Clients.Group(roomID).SendAsync("NewUsers", Context.ConnectionId);
        }

        public async Task SendSignalData(string reciver, string sender, string data)
        {
            await Clients.Client(reciver).SendAsync("RecivedSignalData", reciver, sender, data);
        }

        public async Task SendAnswer(string reciver, string sender, string data)
        {
            await Clients.Client(reciver).SendAsync("RecivedAnserData", reciver, sender, data);
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            if (socketToRoom.TryGetValue(Context.ConnectionId, out var roomID))
            {
                socketToRoom.Remove(Context.ConnectionId);
                if (users.TryGetValue(roomID, out var room))
                {
                    room = room.FindAll(id => id != Context.ConnectionId);
                    users[roomID] = room;
                }
            }

            return base.OnDisconnectedAsync(exception);
        }
    }

}
