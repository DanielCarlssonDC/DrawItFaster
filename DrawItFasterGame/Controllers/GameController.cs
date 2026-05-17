using Microsoft.AspNetCore.Mvc;

namespace DrawItFasterGame.Controllers
{
	public class GameController : Controller
	{
		public IActionResult Room(string gameId = "testgame")
		{
			ViewBag.GameId = gameId;

			return View();
		}
	}
}
