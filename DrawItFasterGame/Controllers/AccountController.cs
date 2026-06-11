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
        public AccountController(DrawItDbContext context)
        {
            _context = context;
        }
        public IActionResult LogIn()
        {
            return View();
        }
        public IActionResult CreateUser()
        {
            return View();
        }
        [HttpPost]
        public IActionResult CreateUser(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.error = "Username and password are required.";
                return View();
            }
            if (_context.Users.Any(u => u.Username == username))
            {
                ViewBag.error = "Username already exists.";
                return View();
            }
            var user = new User
            {
                Username = username,
                ProfilePictureUrl = "https://upload.wikimedia.org/wikipedia/commons/a/ac/Default_pfp.jpg"
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, password);

            _context.Users.Add(user);
            _context.SaveChanges();

            // Automatically log in user after registration
            HttpContext.Session.SetInt32("UserId", user.UserID);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("ProfilePic", user.ProfilePictureUrl ?? "/images/profilepic.png");

            return RedirectToAction("Index", "Home");
        }
    }
}
