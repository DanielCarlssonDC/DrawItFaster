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
    }

    public class GamePlayer
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string ProfilePic { get; set; }
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