using HumanAidTransport.Models; 
using HumanitarianTransport.Data;
using Microsoft.AspNetCore.Mvc;

namespace HumanAidTransport.Controllers
{
    public class RegistrationController : Controller
    {
        private readonly HumanitarianDbContext _context;

        public RegistrationController(HumanitarianDbContext context)
        {
            _context = context;
        }

        public IActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Registration(Carrier carrier)
        {
            if (ModelState.IsValid)
            {
                //Перевірка чи існує наш користувач 
                bool carrierExists = _context.Carriers.Any(c => c.Name == carrier.Name);

                if (!carrierExists)
                {
                    //Додаємо нашого користувача
                    _context.Carriers.Add(carrier);
                    _context.SaveChanges();

                    Carrier addedCarrier = _context.Carriers.FirstOrDefault(c => c.Name == carrier.Name);

                    
                }
            }
            return RedirectToAction("Profile", "Profile");
        }
    }
}

