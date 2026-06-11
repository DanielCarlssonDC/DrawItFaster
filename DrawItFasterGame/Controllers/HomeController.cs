using DrawItFaster.DAL;
using DrawItFasterGame.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace DrawItFasterGame.Controllers
{
    public class HomeController : Controller
    {
        private readonly DrawItDbContext _context;
        public HomeController(DrawItDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Leaderboard()
        {
            var topPlayers = _context.Users
                .OrderByDescending(u => u.Highscore)
                .ToList();

            return View(topPlayers);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
