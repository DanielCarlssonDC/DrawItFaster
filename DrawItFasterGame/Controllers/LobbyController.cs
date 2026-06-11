using DrawItFasterGame.Hubs;
using DrawItFasterGame.Models;
using DrawItFasterGame.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace DrawItFasterGame.Controllers
{
    public class LobbyController : Controller
    {
        private readonly IHubContext<GameHub> _hubContext;
        public LobbyController(IHubContext<GameHub> hubContext)
        {
            _hubContext = hubContext;
        }

        private int? GetUserId() => HttpContext.Session.GetInt32("UserId");
        private string GetUsername() => HttpContext.Session.GetString("Username");
        private string GetProfilePic() => HttpContext.Session.GetString("ProfilePic");

        [HttpPost]
        public IActionResult CreateGame()
        {
            var userId = GetUserId();
            if (userId == null) return RedirectToAction("LogIn", "Account");

            string newCode = GameManager.GenerateGameCode();

            var newGame = new Game
            {
                GameCode = newCode,
                HostId = userId.Value
            };

            // Add host as first player
            newGame.Players.Add(new GamePlayer
            {
                UserId = userId.Value,
                Username = GetUsername(),
                ProfilePic = GetProfilePic()
            });

            // Save the game in server memory
            GameManager.ActiveGames.TryAdd(newCode, newGame);

            return RedirectToAction("GameLobby", new { gameCode = newCode });
        }

        [HttpPost]
        public async Task<IActionResult> JoinGame(string gameCode)
        {
            var userId = GetUserId();
            if (userId == null) return RedirectToAction("LogIn", "Account");

            if (string.IsNullOrEmpty(gameCode) || !GameManager.ActiveGames.ContainsKey(gameCode))
            {
                TempData["ErrorMessage"] = "Invalid game code!";
                return RedirectToAction("Index", "Home");
            }

            var game = GameManager.ActiveGames[gameCode];

            if (game.IsStarted)
            {
                TempData["ErrorMessage"] = "Game has already started";
                return RedirectToAction("Index", "Home");
            }

            if (!game.Players.Any(p => p.UserId == userId.Value))
            {
                game.Players.Add(new GamePlayer
                {
                    UserId = userId.Value,
                    Username = GetUsername(),
                    ProfilePic = GetProfilePic()
                });

                // Send ping to every user in lobby to update player list
                await _hubContext.Clients.Group(gameCode).SendAsync("LobbyPlayersUpdated");
            }

            return RedirectToAction("GameLobby", new { gameCode = gameCode });
        }

        [HttpGet]
        public IActionResult GameLobby(string gameCode)
        {
            var userId = GetUserId();
            if (userId == null) return RedirectToAction("LogIn", "Account");

            if (!GameManager.ActiveGames.ContainsKey(gameCode))
                return RedirectToAction("Index", "Home");

            var game = GameManager.ActiveGames[gameCode];

            ViewBag.Game = game;
            ViewBag.CurrentUserId = userId;

            // Send list of GamePlayers to the view
            return View(game.Players);
        }

        // Method for partial view of players
        public IActionResult PlayersList(string gameCode)
        {
            if (!GameManager.ActiveGames.ContainsKey(gameCode)) return NotFound();

            var game = GameManager.ActiveGames[gameCode];
            ViewBag.Game = game;

            return PartialView("_LobbyPlayersPartial", game.Players);
        }

        [HttpPost]
        public async Task<IActionResult> LeaveGame(string gameCode)
        {
            var userId = GetUserId();
            if (userId == null) return RedirectToAction("LogIn", "Account");

            if (GameManager.ActiveGames.TryGetValue(gameCode, out var game))
            {
                // Find player in list and remove
                var player = game.Players.FirstOrDefault(p => p.UserId == userId.Value);
                if (player != null)
                {
                    game.Players.Remove(player);

                    // Send ping to lobby players
                    await _hubContext.Clients.Group(gameCode).SendAsync("UpdatePlayerList");
                }
            }

            // send player to home page
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> CancelGame(string gameCode)
        {
            var userId = GetUserId();
            if (userId == null) return RedirectToAction("LogIn", "Account");

            if (GameManager.ActiveGames.TryGetValue(gameCode, out var game))
            {
                // Make sure the user deleting game is host
                if (game.HostId == userId.Value)
                {
                    // Send ping to all players that game is deleted
                    await _hubContext.Clients.Group(gameCode).SendAsync("LobbyCancelled");

                    // Delete game from server-memory
                    GameManager.ActiveGames.TryRemove(gameCode, out _);
                }
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> StartGame(string gameCode)
        {
            var userId = GetUserId();
            if (userId == null) return RedirectToAction("LogIn", "Account");

            if (GameManager.ActiveGames.TryGetValue(gameCode, out var game))
            {
                if (game.HostId == userId.Value)
                {
                    // Set game status to "started"
                    game.IsStarted = true;

                    // Send ping to all players in lobby that game has started
                    await _hubContext.Clients.Group(gameCode).SendAsync("GameStarted", gameCode);
                }
            }

            // Send host to game
            return RedirectToAction("Room", "Game", new { gameId = gameCode });
        }
    }
}