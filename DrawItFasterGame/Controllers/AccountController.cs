using DrawItFaster.DAL;
using DrawItFasterGame.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

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

        [HttpPost]
        public IActionResult LogIn(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "You must fill in both username and password";
                return View();
            }

            var user = _context.Users.FirstOrDefault(u => u.Username == username);

            if (user == null)
            {
                ViewBag.Error = "Wrong username or password";
                return View();
            }

            // Check the password
            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);

            if (result == PasswordVerificationResult.Failed)
            {
                ViewBag.Error = "Wrong username or password";
                return View();
            }

            // Process successful login
            HttpContext.Session.SetInt32("UserId", user.UserID);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("ProfilePic", user.ProfilePictureUrl);

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("LogIn", "Account");
        }

        [HttpGet]
        public IActionResult Profile()
        {
            string username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username)) return RedirectToAction("LogIn");

            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            return View(user);
        }

        [HttpPost]
        public IActionResult UpdatePicture(string pictureUrl)
        {
            string username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username)) return RedirectToAction("LogIn");

            // Check if input is empty
            if (string.IsNullOrWhiteSpace(pictureUrl))
            {
                TempData["ErrorMessage"] = "You must enter a link to update your picture.";
                return RedirectToAction("Profile");
            }

            // Check if link is a valid url
            bool isValidUrl = Uri.TryCreate(pictureUrl, UriKind.Absolute, out Uri uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            // Check if url ends with image file type
            string lowerUrl = pictureUrl.ToLower();
            bool isImage = lowerUrl.EndsWith(".jpg") || lowerUrl.EndsWith(".jpeg") || lowerUrl.EndsWith(".png") || lowerUrl.EndsWith(".gif");
            if (!isValidUrl || !isImage)
            {
                TempData["ErrorMessage"] = "The link must be a valid URL ending in .jpg, .png, or .gif.";
                return RedirectToAction("Profile");
            }

            // Proceed to change profile picture
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user != null)
            {
                user.ProfilePictureUrl = pictureUrl;
                _context.SaveChanges();
                HttpContext.Session.SetString("ProfilePic", pictureUrl);
                TempData["SuccessMessage"] = "Profile picture updated successfully!";
            }

            return RedirectToAction("Profile");
        }

        [HttpPost]
        public IActionResult DeleteUser()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId != null)
            {
                var user = _context.Users.FirstOrDefault(u => u.UserID == userId);

                if (user != null)
                {
                    _context.Users.Remove(user);
                    _context.SaveChanges();
                    HttpContext.Session.Clear();
                }
            }
            return RedirectToAction("CreateUser", "Account");
        }
    }
}
