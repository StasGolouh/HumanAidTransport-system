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
        public IActionResult Login(string role, string username, string password)
        {
            if (string.IsNullOrEmpty(role) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError(string.Empty, "Будь ласка, заповніть всі поля.");
                return View();
            }

            if (role == "Carrier")
            {
                // Перевізник
                var carrier = _context.Carriers.FirstOrDefault(c => c.Name == username && c.Password == password);

                if (carrier != null)
                {
                    // Якщо перевізник знайдений, зберігаємо його у сесії
                    HttpContext.Session.SetInt32("CarrierId", carrier.CarrierId);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Неправильне ім'я користувача або пароль для перевізника.");
                    return View();
                }
            }
            else if (role == "Customer")
            {
                // Замовник
                var customer = _context.Customers.FirstOrDefault(c => c.Username == username && c.Password == password);

                if (customer != null)
                {
                    HttpContext.Session.SetInt32("CustomerId", customer.CustomerId);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Неправильне ім'я користувача або пароль для замовника.");
                    return View();
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Будь ласка, виберіть правильну роль.");
                return View();
            }
        }
    }
}
