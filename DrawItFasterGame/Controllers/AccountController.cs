using DrawItFasterGame.Models;
using DrawItFaster.DAL;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DrawItFasterGame.Controllers
{
    public class AccountController : Controller
    {
        private readonly DrawItDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher = new();
        public IActionResult Index()
        {
            return View();
        }
    }
}
