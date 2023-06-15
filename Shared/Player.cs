namespace Rock_Paper_Scissors.Shared
{
    public class Player
    {
        public string? Username { get; set; }

        public string? RoomCode { get; set; }

        public string? Choice { get; set; }

        public string? ConnectionId { get; set; }

        public Player(string username, string roomCode, string connectionId)
        {
            this.Username = username;
            this.RoomCode = roomCode;
            this.ConnectionId = connectionId;
        }

        public Player(string username, string roomCode)
        {
            this.Username = username;
            this.RoomCode = roomCode;
        }

        public Player()
        {

        }
    }
}
