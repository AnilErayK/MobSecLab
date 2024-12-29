using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MobSecLab.Models;
using System.Linq;

namespace MobSecLab.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly MobSecLabContext _context;

        public AdminController(MobSecLabContext context)
        {
            _context = context;
        }

        private bool CheckIfAdmin()
        {
            var username = User.Identity.Name;
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            return (user.Role==1);
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (!CheckIfAdmin())
                return Unauthorized(); // veya RedirectToAction("Login","Auth");

            return View();
        }

        // 1) ÜYE EKLEME
        [HttpGet]
        public IActionResult CreateUser()
        {
            if (!CheckIfAdmin())
                return Unauthorized();

            return View();
        }

        [HttpPost]
        public IActionResult CreateUser(User model)
        {
            if (!CheckIfAdmin())
                return Unauthorized();

            if (ModelState.IsValid)
            {
                // Varsayalım ki parolasını da alıyoruz, isterseniz hash vs. de yapabilirsiniz
                model.Role = 0; // Normal üye ekliyoruz
                _context.Users.Add(model);
                _context.SaveChanges();
                return RedirectToAction("ListUsers");
            }
            return View(model);
        }

        // 2) ÜYE LİSTELEME
        [HttpGet]
        public IActionResult ListUsers()
        {
            if (!CheckIfAdmin())
                return Unauthorized();

            var users = _context.Users.ToList();
            return View(users);
        }

        // 3) ÜYE SİLME
        [HttpPost]
        public IActionResult DeleteUser(int userNo)
        {
            if (!CheckIfAdmin())
                return Unauthorized();

            var user = _context.Users.Find(userNo);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }
            return RedirectToAction("ListUsers");
        }

        // 4) Dosya ve Sonuçları Listeleme
        [HttpGet]
        public IActionResult ListFilesAndResults()
        {
            if (!CheckIfAdmin())
                return Unauthorized();

            // Tüm Files ve Results tablolarını join edip gösterebilirsiniz. 
            // Basit örnek: sadece Results tablosunu çekip modele verelim
            var allResults = _context.Results.ToList();
            return View(allResults);
        }

        // 5) Dosya ve Sonuç SİL
        [HttpPost]
        public IActionResult DeleteResult(int resultsNo)
        {
            if (!CheckIfAdmin())
                return Unauthorized();

            var res = _context.Results.Find(resultsNo);
            if (res != null)
            {
                // Results tablosundan siliyoruz
                _context.Results.Remove(res);

                // Dosyayı da silmek isterseniz Files tablosunda bulup oradan da remove edebilirsiniz.
                var fileEntity = _context.Files.FirstOrDefault(f => f.FileSeq == res.FileSeq);
                if(fileEntity != null)
                    _context.Files.Remove(fileEntity);

                _context.SaveChanges();
            }
            return RedirectToAction("ListFilesAndResults");
        }
    }
}
