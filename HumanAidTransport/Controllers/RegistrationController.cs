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
            var model = new ViewModel
            {
                Carrier = new Carrier(), 
                Volunteer = new Volunteer() 
            };
            return View(model);
        }

        [HttpPost]
        public IActionResult Registration(string role, Carrier carrier, Volunteer customer)
        {
            if (string.IsNullOrEmpty(role))
            {
                ModelState.AddModelError("", "Будь ласка, виберіть роль.");
                return View();
            }

            if (role == "Carrier")
            {
                if (ModelState.IsValid)
                {
                    // Перевіряємо чи вже існує такий перевізник або номер машини
                    bool carrierExists = _context.Carriers.Any(c => c.Name == carrier.Name || c.VehicleNumber == carrier.VehicleNumber);

                    if (!carrierExists)
                    {
                        // Додаємо перевізника в базу
                        _context.Carriers.Add(carrier);
                        _context.SaveChanges();

                        // Отримуємо ID щойно зареєстрованого перевізника
                        Carrier addedCarrier = _context.Carriers.FirstOrDefault(c => c.Name == carrier.Name);

                        // Переадресовуємо в профіль перевізника
                        return RedirectToAction("Profile", "Profile", new { id = addedCarrier.Id });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Перевізник або номер машини вже існує.");
                    }
                }
            }
            else if (role == "Customer")
            {
                if (ModelState.IsValid)
                {
                    // Перевіряємо чи вже існує такий замовник
                    bool customerExists = _context.Volunteer.Any(c => c.Name == customer.Name);

                    if (!customerExists)
                    {
                        // Додаємо замовника в базу
                        _context.Volunteer.Add(customer);
                        _context.SaveChanges();

                        // Отримуємо ID щойно зареєстрованого замовника
                        Volunteer addedCustomer = _context.Volunteer.FirstOrDefault(c => c.Name == customer.Name);

                        // Переадресовуємо в профіль замовника
                        return RedirectToAction("Profile", "Profile", new { id = addedCustomer.Id });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Замовник з таким ім'ям вже існує.");
                    }
                }
            }
            else
            {
                ModelState.AddModelError("", "Невірна роль для реєстрації.");
            }

            return View();
        }
    }
}
