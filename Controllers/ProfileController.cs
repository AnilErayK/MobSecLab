using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobSecLab.Models;
using System.Linq;

namespace MobSecLab.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly MobSecLabContext _context;

        public ProfileController(MobSecLabContext context)
        {
            _context = context;
        }

        public IActionResult Details()
        {
            var username = User.Identity.Name;
            var user = _context.Users.FirstOrDefault(u => u.Username == username);

            if (user == null)
                return RedirectToAction("Login", "Auth");

            return View(user);
        }

        [HttpPost]
        public IActionResult UpdatePassword(string currentPassword, string newPassword)
        {
            var username = User.Identity.Name;
            var user = _context.Users.FirstOrDefault(u => u.Username == username);

            if (user == null || user.Password != currentPassword)
            {
                ViewBag.Error = "Mevcut şifre hatalı!";
                return View("Details", user);
            }

            user.Password = newPassword;
            _context.SaveChanges();

            ViewBag.Success = "Şifre başarıyla güncellendi!";
            return View("Details", user);
        }

        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public IActionResult ChangePassword(string currentPassword, string newPassword)
        {
            var username = User.Identity.Name;
            var user = _context.Users.FirstOrDefault(u => u.Username == username);

            if (user == null || user.Password != currentPassword)
            {
                ViewBag.Error = "Mevcut şifre hatalı!";
                return View();
            }

            user.Password = newPassword;
            _context.SaveChanges();

            ViewBag.Success = "Şifre başarıyla güncellendi!";
            return View();
        }

        [Authorize]
        public IActionResult LogoutConfirmation()
        {
            return View();
        }

        [Authorize]
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
