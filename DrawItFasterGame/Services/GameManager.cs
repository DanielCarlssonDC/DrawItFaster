using System.Collections.Concurrent;

namespace DrawItFasterGame.Services
{
    // More simple models for in-game memory
    public class Game
    {
        public string GameCode { get; set; }
        public int HostId { get; set; }
        public bool IsStarted { get; set; } = false;
        public List<GamePlayer> Players { get; set; } = new();
        public int CurrentRound { get; set; } = 1;
        public int MaxRounds { get; set; } = 3;
        public int CurrentDrawerIndex { get; set; } = 0;
        public string CurrentWord { get; set; } = "";
        public DateTime RoundStartTime { get; set; }

        public bool IsWaitingForWordSelection { get; set; } = true;
    }

    public class GamePlayer
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string ProfilePic { get; set; }
        public int Score { get; set; } = 0;
        public bool HasGuessedCorrectlyThisTurn { get; set; } = false;
    }

    // Server memory
    public static class GameManager
    {
        // List of all active games
        public static ConcurrentDictionary<string, Game> ActiveGames = new();

        // Generate game code
        public static string GenerateGameCode()
        {
            const string chars = "0123456789";
            var random = new Random();
            string newCode;

            do
            {
                newCode = new string(Enumerable.Repeat(chars, 5).Select(s => s[random.Next(s.Length)]).ToArray());
            }
            while (ActiveGames.ContainsKey(newCode));

            return newCode;
        }
    }
}