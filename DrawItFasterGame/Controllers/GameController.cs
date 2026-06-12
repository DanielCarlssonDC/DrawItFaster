using DrawItFasterGame.Services;
using Microsoft.AspNetCore.Mvc;

namespace DrawItFasterGame.Controllers
{
    public class GameController : Controller
    {
        [HttpGet]
        public IActionResult Room(string gameId) // Behåller gameId som parameter för att matcha din befintliga kod
        {
            // Sätter loggan korrekt i headern
            ViewData["IsGameView"] = true;

            if (GameManager.ActiveGames.TryGetValue(gameId, out var game))
            {
                ViewBag.GameId = gameId;
                ViewBag.Username = HttpContext.Session.GetString("Username") ?? "Unknown";

                // Skicka med hela spelobjektet till vyn
                return View(game);
            }

            // Om spelet inte finns, kasta tillbaka till startsidan
            return RedirectToAction("Index", "Home");
        }
    }
}