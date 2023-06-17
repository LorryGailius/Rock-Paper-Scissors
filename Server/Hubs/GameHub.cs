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

            await Task.CompletedTask;
        }

        public async Task Answer(Player player)
        {
            rooms[player.RoomCode].FirstOrDefault(p => p.Username == player.Username).Choice = player.Choice;

            // Check if one player in room
            if (rooms[player.RoomCode].Count == 1)
            {
                await Clients.Group(player.RoomCode).SendAsync("Error", "Opponent left the game");
            }

            // Check if both players have answered
            if (rooms[player.RoomCode].All(p => p.Choice != null))
            {
                // Get winner
                var winner = GetWinner(rooms[player.RoomCode][0], rooms[player.RoomCode][1]);

                // Send winner to players
                await Clients.Group(player.RoomCode).SendAsync("GameEnd", winner);

                // Reset choices
                rooms[player.RoomCode][0].Choice = null;
                rooms[player.RoomCode][1].Choice = null;
            }
        }

        public Player GetWinner(Player p1, Player p2)
        {
            // Check if player 2 wins
            if (p1.Choice == "Paper" && p2.Choice == "Scissors" || p1.Choice == "Rock" && p2.Choice == "Paper" || p1.Choice == "Scissors" && p2.Choice == "Rock")
            {
                return p2;
            }

            // Check if it's a tie
            if(p1.Choice == p2.Choice)
            {
                Player tie = new Player();
                tie.Username = ".:Tie:.";
                return tie;
            }

            return p1;
        }

        public async Task JoinRoom(Player player)
        {
            // Add room if it doesn't exist
            if (!rooms.ContainsKey(player.RoomCode))
            {
                rooms.Add(player.RoomCode, new List<Player>());
            }

            // Check if username is taken
            if (rooms[player.RoomCode].Any(p => p.Username == player.Username))
            {
                await Clients.Caller.SendAsync("Error", "Username is taken");
                return;
            }

            // Check if room is full
            if (rooms[player.RoomCode].Count < 2)
            {
                rooms[player.RoomCode].Add(new Player(player.Username, player.RoomCode, Context.ConnectionId));
            }
            else
            {
                await Clients.Caller.SendAsync("Error", "Room is full");
                return;
            }

            // Add user to group
            await Groups.AddToGroupAsync(Context.ConnectionId, player.RoomCode);
            await Clients.Caller.SendAsync("Success");

            // Check if room is full
            if (rooms[player.RoomCode].Count == 2)
            {
                await Clients.Group(player.RoomCode).SendAsync("StartGame");
            }
        }
    }
}
