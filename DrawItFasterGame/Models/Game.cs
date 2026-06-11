namespace DrawItFasterGame.Models
{
    public class Game
    {
        public int GameID { get; set; }
        public int HostID { get; set; }
        public User Host { get; set; }
        public int Status { get; set; } = 0;

        public int? CurrentGamePlayerID { get; set; }

        public ICollection<GamePlayer> GamePlayers { get; set; } = new List<GamePlayer>();
    }
}