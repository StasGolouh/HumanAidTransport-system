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
                // Перевіряємо чи є вже такий перевізник або машина
                bool carrierExists = _context.Carriers.Any(c => c.Name == carrier.Name || c.VehicleNumber == carrier.VehicleNumber);

                if (!carrierExists)
                {
                    // Додаємо перевізника в базу
                    _context.Carriers.Add(carrier);
                    _context.SaveChanges();

                    // Отримуємо ID щойно зареєстрованого перевізника
                    Carrier addedCarrier = _context.Carriers.FirstOrDefault(c => c.Name == carrier.Name);

                    // Переадресовуємо в профіль
                    return RedirectToAction("Profile", "Profile", new { id = addedCarrier.CarrierId });
                }
                else
                {
                    ModelState.AddModelError("", "Перевізник або номер машини вже існує.");
                }
            }

            return View(carrier);
        }
    }
}
