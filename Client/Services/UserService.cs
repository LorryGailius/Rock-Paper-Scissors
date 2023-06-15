using Rock_Paper_Scissors.Shared;

namespace Rock_Paper_Scissors.Client.Services
{
    public class UserService : IUserService
    {
        public Player User { get; set; } = new Player();
    }
}