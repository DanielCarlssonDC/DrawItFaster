using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;

namespace DrawItFasterGame.Controllers
{
	public class GameController : Controller
	{
		public IActionResult Room(string gameId)
		{

			if (string.IsNullOrWhiteSpace(gameId)){
				gameId = Guid.NewGuid().ToString("N").Substring(0, 6);
			}

			ViewBag.GameId = gameId;

			return View();
		}
	}
}
