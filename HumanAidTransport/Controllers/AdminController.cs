using HumanitarianTransport.Data;
using Microsoft.AspNetCore.Mvc;

namespace HumanAidTransport.Controllers
{
    public class AdminController : Controller
    {
        private readonly HumanitarianDbContext _context;

        public AdminController(HumanitarianDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult AdminRegister()
        {
            var currentUser = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrEmpty(currentUser) || !_context.Admins.Any(a => a.Name == currentUser))
            {
                TempData["Message"] = "Ви не маєте доступу для створення адміна.";
                return RedirectToAction("Index", "Home");
            }

            return View("~/Views/Register/AdminRegister.cshtml");
        }

        public IActionResult AdminLogin()
        {
            return View("~/Views/Login/AdminLogin.cshtml");
        }

        [HttpPost]
        public IActionResult AdminLogin(string name, string password)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError(string.Empty, "Будь ласка, заповніть усі поля.");
                return View("~/Views/Login/AdminLogin.cshtml");
            }

            // Валідація імені
            if (!System.Text.RegularExpressions.Regex.IsMatch(name ?? "", @"^[a-zA-Zа-яА-ЯіІїЇєЄґҐ0-9\s]{2,}$"))
            {
                ModelState.AddModelError("name", "Ім'я може містити лише літери, цифри та пробіли (мінімум 2 символи).");
                return View("~/Views/Login/AdminLogin.cshtml");
            }

            // Валідація пароля
            if (password.Length < 8 || password.Contains(" "))
            {
                ModelState.AddModelError("password", "Пароль має бути не менше 8 символів і не містити пробілів.");
                return View("~/Views/Login/AdminLogin.cshtml");
            }

            var admin = _context.Admins.FirstOrDefault(a => a.Name == name && a.Password == password);

            if (admin != null)
            {
                AdminProfileController.Admin = admin;
                HttpContext.Session.SetString("UserName", name);
                return RedirectToAction("AdminProfile", "AdminProfile");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Неправильний логін або пароль для волонтера.");
                return View("~/Views/Login/AdminLogin.cshtml");
            }
        }
    }
}

