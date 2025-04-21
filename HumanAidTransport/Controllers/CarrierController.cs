using HumanitarianTransport.Data;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace HumanAidTransport.Controllers
{
    public class CarrierController : Controller
    {
        private readonly HumanitarianDbContext _context;

        public CarrierController(HumanitarianDbContext context)
        {
            _context = context;
        }

        // =============== GET: Registration View ================
        public IActionResult CarrierRegistration()
        {
            return View("~/Views/Registration/CarrierRegistration.cshtml");
        }

        // =============== POST: Carrier Registration ===============
        [HttpPost]
        public IActionResult CarrierRegistration(Carrier carrier)
        {
            if (ModelState.IsValid)
            {
                ValidateCarrierInput(carrier);
                CheckForDuplicates(carrier);

                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Carriers.Add(carrier);
                        _context.SaveChanges();

                        TempData["RegistMessage"] = "Реєстрація пройшла успішно, будь ласка, залогінтеся.";
                        return RedirectToAction("CarrierProfile", "CarrierProfile");
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Помилка збереження перевізника: " + ex.Message);
                    }
                }
            }

            return View("~/Views/Registration/CarrierRegistration.cshtml");
        }

        // =============== GET: Login View ===============
        public IActionResult CarrierLogin()
        {
            return View("~/Views/Login/CarrierLogin.cshtml");
        }

        // =============== POST: Carrier Login ===============
        [HttpPost]
        public IActionResult CarrierLogin(string name, string password)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError(string.Empty, "Будь ласка, заповніть усі поля.");
            }
            else
            {
                if (!IsValidName(name))
                    ModelState.AddModelError("name", "Ім'я може містити лише літери, цифри та пробіли (мінімум 2 символи).");

                if (!IsValidPassword(password))
                    ModelState.AddModelError("password", "Пароль має бути не менше 8 символів і не містити пробілів.");
            }

            if (!ModelState.IsValid)
                return View("~/Views/Login/CarrierLogin.cshtml");

            var carrier = _context.Carriers.FirstOrDefault(c => c.Name == name && c.Password == password);

            if (carrier != null)
            {
                CarrierProfileController.Carrier = carrier;
                HttpContext.Session.SetString("UserName", name);
                return RedirectToAction("CarrierProfile", "CarrierProfile");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Неправильний логін або пароль перевізника.");
                return View("~/Views/Login/CarrierLogin.cshtml");
            }
        }

        // =============== Validation Helpers ===============

        private void ValidateCarrierInput(Carrier carrier)
        {
            if (!IsValidName(carrier.Name))
                ModelState.AddModelError("Name", "Ім'я може містити лише літери, цифри та пробіли.");

            if (!IsValidPassword(carrier.Password))
                ModelState.AddModelError("Password", "Пароль має бути не менше 8 символів і не містити пробілів.");

            if (!IsValidPhone(carrier.Contacts))
                ModelState.AddModelError("Contacts", "Номер телефону має бути у форматі +380XXXXXXXXX.");

            if (!IsValidVehicleNumber(carrier.VehicleNumber))
                ModelState.AddModelError("VehicleNumber", "Номер транспортного засобу має містити рівно 8 символів без пробілів.");

            if (!IsValidDimensions(carrier.Dimensions))
                ModelState.AddModelError("Dimensions", "Розміри мають бути у форматі 10x10x10 без літер, знаків мінус і пробілів.");
        }

        private void CheckForDuplicates(Carrier carrier)
        {
            // Перевірка на дублікати серед перевізників
            var existingCarr = _context.Carriers.FirstOrDefault(c =>
                c.Name == carrier.Name ||
                c.VehicleNumber == carrier.VehicleNumber ||
                c.Contacts == carrier.Contacts);

            if (existingCarr != null)
            {
                if (existingCarr.Name == carrier.Name)
                    ModelState.AddModelError("Name", "Перевізник із такою назвою вже існує.");

                if (existingCarr.VehicleNumber == carrier.VehicleNumber)
                    ModelState.AddModelError("VehicleNumber", "Перевізник із таким номером автомобіля вже існує.");

                if (existingCarr.Contacts == carrier.Contacts)
                    ModelState.AddModelError("Contacts", "Оператор із таким номером телефону вже існує.");
            }

            // 🔍 Додаткова перевірка — чи таке ім’я вже є серед волонтерів
            var volunteerWithSameName = _context.Volunteers.FirstOrDefault(v => v.Name == carrier.Name);
            if (volunteerWithSameName != null)
            {
                ModelState.AddModelError("Name", "Ім’я вже використовується волонтером. Оберіть інше");
            }
        }

        // =============== Regex Helpers ===============

        private bool IsValidName(string name) =>
            Regex.IsMatch(name ?? "", @"^[a-zA-Zа-яА-ЯіІїЇєЄґҐ0-9\s]{2,}$");

        private bool IsValidPassword(string password) =>
            !string.IsNullOrWhiteSpace(password) && password.Length >= 8 && !password.Contains(" ");

        private bool IsValidPhone(string phone) =>
            Regex.IsMatch(phone ?? "", @"^\+380\d{9}$");

        private bool IsValidVehicleNumber(string number) =>
            !string.IsNullOrWhiteSpace(number) && number.Length == 8 && !number.Contains(" ");

        private bool IsValidDimensions(string dim) =>
            Regex.IsMatch(dim ?? "", @"^\d+x\d+x\d+$");
    }
}
