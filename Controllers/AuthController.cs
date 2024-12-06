using Microsoft.AspNetCore.Mvc;
using MobSecLab.Models;
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
                return RedirectToAction("Index", "Home");
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
            if (ModelState.IsValid)
            {
                _context.Users.Add(user);
                _context.SaveChanges();
                return RedirectToAction("Login");
            }
            return View(user);
        }
    }
}
