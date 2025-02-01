using HumanAidTransport.Models;
using HumanitarianTransport.Data;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

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
            var model = new RegistrationViewModel
            {
                Carrier = new Carrier(), 
                Customer = new Customer() 
            };
            return View(model);
        }

        [HttpPost]
        public IActionResult Registration(string role, Carrier carrier, Customer customer)
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
                        return RedirectToAction("Profile", "Profile", new { id = addedCarrier.CarrierId });
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
                    bool customerExists = _context.Customers.Any(c => c.Username == customer.Username);

                    if (!customerExists)
                    {
                        // Додаємо замовника в базу
                        _context.Customers.Add(customer);
                        _context.SaveChanges();

                        // Отримуємо ID щойно зареєстрованого замовника
                        Customer addedCustomer = _context.Customers.FirstOrDefault(c => c.Username == customer.Username);

                        // Переадресовуємо в профіль замовника
                        return RedirectToAction("Profile", "Profile", new { id = addedCustomer.CustomerId });
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
