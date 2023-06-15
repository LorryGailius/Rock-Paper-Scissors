using Microsoft.AspNetCore.SignalR;
using Rock_Paper_Scissors.Shared;

namespace Rock_Paper_Scissors.Server.Hubs
{
    public class GameHub : Hub
    {
        private static Dictionary<string, List<Player>> rooms = new Dictionary<string, List<Player>>();

        public void Send(string message)
        {
            Clients.All.SendAsync("Send", message);
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);

            await DeletePlayer(Context.ConnectionId);
        }

        public async Task DeletePlayer(string connectionString)
        {
            // Find player
            var player = rooms.SelectMany(r => r.Value).FirstOrDefault(p => p.ConnectionId == connectionString);

            if (player != null)
            {
                // Remove player from room
                rooms[player.RoomCode].Remove(player);

                // Remove player from group
                await Groups.RemoveFromGroupAsync(player.ConnectionId, player.RoomCode);

                // Check if room is empty
                if (rooms[player.RoomCode].Count == 0)
                {
                    rooms.Remove(player.RoomCode);
                }
            }
            else
            {
                Console.WriteLine("Player not found");
            }

            await Task.CompletedTask;
        }


        public async Task JoinRoom(Player player)
        {
            // Add room if it doesn't exist
            if (!rooms.ContainsKey(player.RoomCode))
            {
                rooms.Add(player.RoomCode, new List<Player>());
            }

            if(player.ConnectionId is null)
            {

                // Add user to room
                rooms[player.RoomCode].Add(new Player(player.Username, player.RoomCode, Context.ConnectionId));
            }
            else
            {
                // Add user to room
                rooms[player.RoomCode].Add(player);
            }

            // Print how many rooms are active
            Console.WriteLine($"\nThere are {rooms.Count} rooms active");

            Console.WriteLine("-------------------------------------------------------------------------------");

            // Print all rooms and their players
            foreach (var room in rooms)
            {
                Console.WriteLine($"Room {room.Key} has {room.Value.Count} players");

                foreach (var p in room.Value)
                {
                    Console.WriteLine($"Player {p.Username} is in room {p.RoomCode}");
                }


                Console.WriteLine("-------------------------------------------------------------------------------");
            }

            // Add user to group
            await Groups.AddToGroupAsync(Context.ConnectionId, player.RoomCode);
            await Clients.Group(player.RoomCode).SendAsync("ReceiveMessage", $"{player.Username} joined {player.ConnectionId}");
        }
    }
}
