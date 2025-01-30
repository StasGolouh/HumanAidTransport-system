using HumanAidTransport.Models;
using HumanitarianTransport.Data;
using Microsoft.AspNetCore.Mvc;


namespace HumanAidTransport.Controllers
{
    public class LoginController : Controller
    {
        private readonly HumanitarianDbContext _context;

        public LoginController(HumanitarianDbContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(Carrier carrier)
        {
            var carrierExists = _context.Carriers.FirstOrDefault(c => c.Name == carrier.Name && c.Password == carrier.Password);

            if (carrierExists != null)
            {
                ProfileController.Carrier = carrierExists;
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Неправильне ім'я користувача або пароль");
                ModelState.AddModelError(string.Empty, "Або Ваш акаунт не є зареєстрований");
                return View();
            }
        }
    }
}
