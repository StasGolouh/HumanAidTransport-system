using HumanitarianTransport.Data;
using Microsoft.AspNetCore.Mvc;


namespace HumanAidTransport.Controllers
{
    public class CarrierController : Controller
    {
        private readonly HumanitarianDbContext _context;

        public CarrierController(HumanitarianDbContext context)
        {
            _context = context;
        }

        public IActionResult CarrierRegistration()
        {
            return View("~/Views/Registration/CarrierRegistration.cshtml");
        }

        [HttpPost]
        public IActionResult CarrierRegistration(Carrier carrier)
        {
            if (ModelState.IsValid)
            {
                // Перевірка імені — тільки літери, мінімум 2 символи
                if (!System.Text.RegularExpressions.Regex.IsMatch(carrier.Name ?? "", @"^[a-zA-Zа-яА-ЯіІїЇєЄґҐ0-9\s]{2,}$"))
                {
                    ModelState.AddModelError("Name", "Name can contain letters, numbers, and spaces only.");
                }

                // Пароль — мінімум 8 символів, без пробілів
                if (string.IsNullOrWhiteSpace(carrier.Password) || carrier.Password.Length < 8 || carrier.Password.Contains(" "))
                {
                    ModelState.AddModelError("Password", "Password must be at least 8 characters long and contain no spaces.");
                }

                // Номер телефону — простий приклад: починається з +380, далі 9 цифр
                if (!System.Text.RegularExpressions.Regex.IsMatch(carrier.Contacts ?? "", @"^\+380\d{9}$"))
                {
                    ModelState.AddModelError("Contacts", "Phone number must be in the format +380XXXXXXXXX.");
                }

                // Номер авто — рівно 8 символів, без пробілів
                if (string.IsNullOrWhiteSpace(carrier.VehicleNumber) || carrier.VehicleNumber.Length != 8 || carrier.VehicleNumber.Contains(" "))
                {
                    ModelState.AddModelError("VehicleNumber", "Vehicle number must be exactly 8 characters long with no spaces.");
                }

                // Dimensions — формат 10x10x10 (лише цифри та 'x')
                if (!System.Text.RegularExpressions.Regex.IsMatch(carrier.Dimensions ?? "", @"^\d+x\d+x\d+$"))
                {
                    ModelState.AddModelError("Dimensions", "Dimensions must be in the format 10x10x10 without letters, minus signs, or spaces.");
                }

                // Перевірка унікальності
                bool carrierExists = _context.Carriers.Any(c => c.Name == carrier.Name);
                if (carrierExists)
                {
                    ModelState.AddModelError("Name", "A carrier with this name already exists.");
                }

                bool vehicleNumberExists = _context.Carriers.Any(c => c.VehicleNumber == carrier.VehicleNumber);
                if (vehicleNumberExists)
                {
                    ModelState.AddModelError("VehicleNumber", "A carrier with this vehicle number already exists.");
                }

                bool phoneNumberExists = _context.Carriers.Any(c => c.Contacts == carrier.Contacts);
                if (phoneNumberExists)
                {
                    ModelState.AddModelError("Contacts", "A carrier with this phone number already exists.");
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Carriers.Add(carrier);
                        _context.SaveChanges();

                        TempData["RegistMessage"] = "Registration was successful, please log in.";
                        return RedirectToAction("CarrierProfile", "CarrierProfile");
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Error saving carrier: " + ex.Message);
                    }
                }
            }

            return View("~/Views/Registration/CarrierRegistration.cshtml");
        }


        //=============================Login====================================

        public IActionResult CarrierLogin()
        {
            return View("~/Views/Login/CarrierLogin.cshtml");
        }

        [HttpPost]
        public IActionResult CarrierLogin(string name, string password)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError(string.Empty, "Please fill in all fields.");
                return View("~/Views/Login/CarrierLogin.cshtml");
            }

            // Перевірка імені — літери, цифри, пробіли, мінімум 2 символи
            if (!System.Text.RegularExpressions.Regex.IsMatch(name ?? "", @"^[a-zA-Zа-яА-ЯіІїЇєЄґҐ0-9\s]{2,}$"))
            {
                ModelState.AddModelError("name", "Name can contain letters, numbers, and spaces only (min 2 characters).");
                return View("~/Views/Login/CarrierLogin.cshtml");
            }

            // Перевірка пароля — мінімум 8 символів, без пробілів
            if (password.Length < 8 || password.Contains(" "))
            {
                ModelState.AddModelError("password", "Password must be at least 8 characters and contain no spaces.");
                return View("~/Views/Login/CarrierLogin.cshtml");
            }

            var carrier = _context.Carriers.FirstOrDefault(c => c.Name == name && c.Password == password);

            if (carrier != null)
            {
                CarrierProfileController.Carrier = carrier;
                HttpContext.Session.SetString("UserName", name);
                return RedirectToAction("CarrierProfile", "CarrierProfile");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Incorrect username or password for the carrier.");
                return View("~/Views/Login/CarrierLogin.cshtml");
            }
        }
    }
}
