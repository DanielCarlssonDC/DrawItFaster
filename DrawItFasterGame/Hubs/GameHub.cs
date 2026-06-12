using DrawItFaster.DAL;
using DrawItFasterGame.Services;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DrawItFasterGame.Hubs
{
    public class GameHub : Hub
    {
        private readonly DrawItDbContext _context;

        public GameHub(DrawItDbContext context)
        {
            _context = context;
        }

        public async Task JoinGame(string gameCode, string username)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, gameCode);
        }

        public async Task SendDrawingData(string gameCode, object drawingData)
        {
            await Clients.OthersInGroup(gameCode).SendAsync("ReceiveDrawingData", drawingData);
        }

        public async Task ClearCanvas(string gameCode)
        {
            await Clients.OthersInGroup(gameCode).SendAsync("CanvasCleared");
        }

        public async Task StartGame(string gameCode)
        {
            if (GameManager.ActiveGames.TryGetValue(gameCode, out var game))
            {
                if (game.Players.Count < 2) return;

                game.IsStarted = true;
                game.CurrentRound = 1;
                game.CurrentDrawerIndex = 0;

                await StartTurn(gameCode, game);
            }
        }

        private async Task StartTurn(string gameCode, Game game)
        {
            game.CurrentWord = "";
            game.IsWaitingForWordSelection = true;

            foreach (var p in game.Players) p.HasGuessedCorrectlyThisTurn = false;

            string currentDrawer = game.Players[game.CurrentDrawerIndex].Username;

            await Clients.Group(gameCode).SendAsync("CanvasCleared");
            await Clients.Group(gameCode).SendAsync("DrawerSelected", currentDrawer, game.CurrentRound);
        }

        public async Task SelectWord(string gameCode, string username, string word)
        {
            if (GameManager.ActiveGames.TryGetValue(gameCode, out var game))
            {
                game.CurrentWord = word;
                game.IsWaitingForWordSelection = false;
                game.RoundStartTime = DateTime.UtcNow;

                await Clients.Group(gameCode).SendAsync("WordSelected", 90);
            }
        }

        public async Task SendGuess(string gameCode, string username, string guess)
        {
            if (!GameManager.ActiveGames.TryGetValue(gameCode, out var game) || game.IsWaitingForWordSelection)
            {
                await Clients.Group(gameCode).SendAsync("ReceiveMessage", username, guess);
                return;
            }

            var guessingPlayer = game.Players.FirstOrDefault(p => p.Username == username);
            if (guessingPlayer == null || guessingPlayer.HasGuessedCorrectlyThisTurn) return;

            if (guess.Trim().Equals(game.CurrentWord, StringComparison.OrdinalIgnoreCase))
            {
                guessingPlayer.HasGuessedCorrectlyThisTurn = true;

                double secondsPassed = Math.Clamp((DateTime.UtcNow - game.RoundStartTime).TotalSeconds, 0, 90);
                int pointsToAward = 500 - (int)((secondsPassed / 90.0) * 400);
                guessingPlayer.Score += pointsToAward;

                var drawer = game.Players[game.CurrentDrawerIndex];
                drawer.Score += 50;

                await Clients.Group(gameCode).SendAsync("CorrectGuess", username, guessingPlayer.Score, drawer.Username, drawer.Score);

                // If everyone has guessed the word, end round
                if (game.Players.Count(p => p.HasGuessedCorrectlyThisTurn) >= game.Players.Count - 1)
                {
                    await EndTurn(gameCode, "Everyone guessed the word!");
                }
            }
            else
            {
                await Clients.Group(gameCode).SendAsync("ReceiveMessage", username, guess);
            }
        }

        // Client calls this method when timer hits 0
        public async Task TimeUp(string gameCode)
        {
            await EndTurn(gameCode, "Time's up!");
        }

        // Sends message to chat and pauses game for 5 seconds
        private async Task EndTurn(string gameCode, string reason)
        {
            if (GameManager.ActiveGames.TryGetValue(gameCode, out var game))
            {
                // Prevent method from being called more than once
                if (game.IsWaitingForWordSelection) return;
                game.IsWaitingForWordSelection = true;

                await Clients.Group(gameCode).SendAsync("TurnEnded", game.CurrentWord, reason);

                // Wait 5 seconds
                await Task.Delay(5000);

                await NextTurn(gameCode);
            }
        }

        public async Task NextTurn(string gameCode)
        {
            if (GameManager.ActiveGames.TryGetValue(gameCode, out var game))
            {
                game.CurrentDrawerIndex++;

                if (game.CurrentDrawerIndex >= game.Players.Count)
                {
                    game.CurrentDrawerIndex = 0;
                    game.CurrentRound++;

                    if (game.CurrentRound > game.MaxRounds)
                    {
                        // Game is over, save scores as highscores
                        foreach (var p in game.Players)
                        {
                            var user = _context.Users.FirstOrDefault(u => u.Username == p.Username);
                            if (user != null)
                            {
                                // Only save highscore if it is higher
                                if (p.Score > user.Highscore)
                                {
                                    user.Highscore = p.Score;
                                }
                            }
                        }
                        await _context.SaveChangesAsync();

                        var sortedPlayers = game.Players.OrderByDescending(p => p.Score).ToList();
                        await Clients.Group(gameCode).SendAsync("GameOver", sortedPlayers);
                        return;
                    }
                }

                await StartTurn(gameCode, game);
            }
        }
    }
}