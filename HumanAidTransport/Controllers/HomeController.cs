using System.Diagnostics;
using HumanAidTransport.Models;
using HumanitarianTransport.Data;
using Microsoft.AspNetCore.Mvc;

namespace HumanAidTransport.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HumanitarianDbContext _context;

        public HomeController(ILogger<HomeController> logger, HumanitarianDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

            [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult MainProfile()
        {
            var name = HttpContext.Session.GetString("UserName");

            if (string.IsNullOrEmpty(name))
            {
                TempData["Message"] = "You need to login or sign up for Volunteer or Carrier.";
                return RedirectToAction("Index", "Home");
            }

            var carrier = _context.Carriers.FirstOrDefault(c => c.Name == name);
            var volunteer = _context.Volunteers.FirstOrDefault(v => v.Name == name);

            if (carrier != null)
            {
                // Перенаправляємо на профіль перевізника
                return RedirectToAction("CarrierProfile", "CarrierProfile", new { id = carrier.Id });
            }
            else if (volunteer != null)
            {
                // Перенаправляємо на профіль волонтера
                return RedirectToAction("VolunteerProfile", "VolunProfile", new { id = volunteer.Id });
            }
            else
            {
                // Якщо користувач не знайдений, перенаправляємо на головну сторінку
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
