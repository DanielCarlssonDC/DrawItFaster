namespace DrawItFasterGame.Models
{
    public class GamePlayer
    {
        public int GamePlayerId { get; set; }
        public int GameID { get; set; }
        public Game Game { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public int TotalScore { get; set; }
    }
}