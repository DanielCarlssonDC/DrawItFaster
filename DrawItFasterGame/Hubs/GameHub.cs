using DrawItFasterGame.Services;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DrawItFasterGame.Hubs
{
    public class GameHub : Hub
    {
        // Join the SignalR group for the specific game room
        public async Task JoinGame(string gameCode, string username)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, gameCode);
        }

        // Forward drawing data to all OTHER players in the room
        public async Task SendDrawingData(string gameCode, object drawingData)
        {
            await Clients.OthersInGroup(gameCode).SendAsync("ReceiveDrawingData", drawingData);
        }

        // Notify others to clear their canvas
        public async Task ClearCanvas(string gameCode)
        {
            await Clients.OthersInGroup(gameCode).SendAsync("CanvasCleared");
        }

        // Called by the host to start the entire game
        public async Task StartGame(string gameCode)
        {
            if (GameManager.ActiveGames.TryGetValue(gameCode, out var game))
            {
                if (game.Players.Count < 2) return; // Need at least 2 players to play

                game.IsStarted = true;
                game.CurrentRound = 1;
                game.CurrentDrawerIndex = 0;

                await StartTurn(gameCode, game);
            }
        }

        // Starts a new turn for the current drawer
        private async Task StartTurn(string gameCode, Game game)
        {
            game.CurrentWord = "";
            game.IsWaitingForWordSelection = true;

            // Reset guess status for all players
            foreach (var p in game.Players)
            {
                p.HasGuessedCorrectlyThisTurn = false;
            }

            string currentDrawer = game.Players[game.CurrentDrawerIndex].Username;

            // Clear the canvas for the new round
            await Clients.Group(gameCode).SendAsync("CanvasCleared");

            // Tell clients who the new drawer is
            await Clients.Group(gameCode).SendAsync("DrawerSelected", currentDrawer, game.CurrentRound);
        }

        // Called by the drawer when they pick a word from the 3 options
        public async Task SelectWord(string gameCode, string username, string word)
        {
            if (GameManager.ActiveGames.TryGetValue(gameCode, out var game))
            {
                game.CurrentWord = word;
                game.IsWaitingForWordSelection = false;

                // IMPORTANT: Save the start time to calculate points later!
                game.RoundStartTime = DateTime.UtcNow;

                // Start the 90 second timer on the clients
                await Clients.Group(gameCode).SendAsync("WordSelected", 90);
            }
        }

        // Handles chat messages and guesses
        public async Task SendGuess(string gameCode, string username, string guess)
        {
            if (!GameManager.ActiveGames.TryGetValue(gameCode, out var game))
                return;

            // If the drawer hasn't picked a word yet, treat everything as normal chat
            if (game.IsWaitingForWordSelection)
            {
                await Clients.Group(gameCode).SendAsync("ReceiveMessage", username, guess);
                return;
            }

            var guessingPlayer = game.Players.FirstOrDefault(p => p.Username == username);

            // If player doesn't exist or already guessed correctly, ignore
            if (guessingPlayer == null || guessingPlayer.HasGuessedCorrectlyThisTurn)
                return;

            // Check if the guess is correct (case insensitive)
            if (guess.Trim().Equals(game.CurrentWord, StringComparison.OrdinalIgnoreCase))
            {
                guessingPlayer.HasGuessedCorrectlyThisTurn = true;

                // Calculate points based on time (100 to 500 points)
                double secondsPassed = (DateTime.UtcNow - game.RoundStartTime).TotalSeconds;

                // Prevent negative numbers if latency causes seconds to go above 90
                secondsPassed = Math.Clamp(secondsPassed, 0, 90);

                // Math: 500 max, minus up to 400 points depending on time spent
                int pointsToAward = 500 - (int)((secondsPassed / 90.0) * 400);
                guessingPlayer.Score += pointsToAward;

                // Give the drawer a flat 50 points per correct guesser
                var drawer = game.Players[game.CurrentDrawerIndex];
                drawer.Score += 50;

                // Send the correct guess event (Client JS will color this green)
                await Clients.Group(gameCode).SendAsync("CorrectGuess", username, guessingPlayer.Score, drawer.Username, drawer.Score);

                // If everyone except the drawer has guessed correctly, end turn early
                if (game.Players.Count(p => p.HasGuessedCorrectlyThisTurn) >= game.Players.Count - 1)
                {
                    await NextTurn(gameCode);
                }
            }
            else
            {
                // Incorrect guess, just show it in chat as a normal message
                await Clients.Group(gameCode).SendAsync("ReceiveMessage", username, guess);
            }
        }

        // Moves the game to the next drawer or next round
        public async Task NextTurn(string gameCode)
        {
            if (GameManager.ActiveGames.TryGetValue(gameCode, out var game))
            {
                game.CurrentDrawerIndex++;

                // If everyone has drawn once, increase the round counter
                if (game.CurrentDrawerIndex >= game.Players.Count)
                {
                    game.CurrentDrawerIndex = 0;
                    game.CurrentRound++;

                    // If max rounds reached, end the game
                    if (game.CurrentRound > game.MaxRounds)
                    {
                        var sortedPlayers = game.Players.OrderByDescending(p => p.Score).ToList();
                        await Clients.Group(gameCode).SendAsync("GameOver", sortedPlayers);
                        return;
                    }
                }

                // Start the next turn
                await StartTurn(gameCode, game);
            }
        }
    }
}