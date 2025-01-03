using Microsoft.AspNetCore.Mvc;
using MobSecLab.Models;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Linq;

namespace MobSecLab.Controllers
{
    public class AuthController : Controller
    {
        private readonly MobSecLabContext _context;

        public AuthController(MobSecLabContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var user = _context.Users.SingleOrDefault(u => u.Username == username && u.Password == password);
            if (user != null)
            {
                var claims = new List<Claim> { new Claim(ClaimTypes.Name, username) };
                var identity = new ClaimsIdentity(claims, "CookieAuth");
                var principal = new ClaimsPrincipal(identity);
                HttpContext.SignInAsync("CookieAuth", principal);
                if (user.Role==0)
                    return RedirectToAction("Dashboard", "Profile");
                if (user.Role==1)
                    return RedirectToAction("Index", "Admin");
            }
            ViewBag.Error = "Geçersiz giriş bilgileri!";
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(User user)
        {
            // Kullanıcı adı benzersizlik kontrolü
            if (_context.Users.Any(u => u.Username == user.Username))
            {
                ViewBag.Error = "Bu kullanıcı adı zaten alınmış. Lütfen başka bir kullanıcı adı seçin.";
                return View(user);
            }

            if (ModelState.IsValid)
            {
                _context.Users.Add(user);
                _context.SaveChanges();

                var claims = new List<Claim> { new Claim(ClaimTypes.Name, user.Username) };
                var identity = new ClaimsIdentity(claims, "CookieAuth");
                var principal = new ClaimsPrincipal(identity);
                HttpContext.SignInAsync("CookieAuth", principal);

                return RedirectToAction("Dashboard", "Profile");
            }
            return View(user);
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync("CookieAuth");
            return RedirectToAction("Login", "Auth");
        }

    }
}
