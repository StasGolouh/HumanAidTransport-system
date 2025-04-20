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
                ModelState.AddModelError(string.Empty, "Please fill in all fields.");
            }
            else
            {
                if (!IsValidName(name))
                    ModelState.AddModelError("name", "Name can contain letters, numbers, and spaces only (min 2 characters).");

                if (!IsValidPassword(password))
                    ModelState.AddModelError("password", "Password must be at least 8 characters and contain no spaces.");
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
                ModelState.AddModelError(string.Empty, "Incorrect username or password for the carrier.");
                return View("~/Views/Login/CarrierLogin.cshtml");
            }
        }

        // =============== Validation Helpers ===============

        private void ValidateCarrierInput(Carrier carrier)
        {
            if (!IsValidName(carrier.Name))
                ModelState.AddModelError("Name", "Name can contain letters, numbers, and spaces only.");

            if (!IsValidPassword(carrier.Password))
                ModelState.AddModelError("Password", "Password must be at least 8 characters long and contain no spaces.");

            if (!IsValidPhone(carrier.Contacts))
                ModelState.AddModelError("Contacts", "Phone number must be in the format +380XXXXXXXXX.");

            if (!IsValidVehicleNumber(carrier.VehicleNumber))
                ModelState.AddModelError("VehicleNumber", "Vehicle number must be exactly 8 characters long with no spaces.");

            if (!IsValidDimensions(carrier.Dimensions))
                ModelState.AddModelError("Dimensions", "Dimensions must be in the format 10x10x10 without letters, minus signs, or spaces.");
        }

        private void CheckForDuplicates(Carrier carrier)
        {
            var existing = _context.Carriers.FirstOrDefault(c =>
                c.Name == carrier.Name ||
                c.VehicleNumber == carrier.VehicleNumber ||
                c.Contacts == carrier.Contacts);

            if (existing != null)
            {
                if (existing.Name == carrier.Name)
                    ModelState.AddModelError("Name", "A carrier with this name already exists.");

                if (existing.VehicleNumber == carrier.VehicleNumber)
                    ModelState.AddModelError("VehicleNumber", "A carrier with this vehicle number already exists.");

                if (existing.Contacts == carrier.Contacts)
                    ModelState.AddModelError("Contacts", "A carrier with this phone number already exists.");
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
