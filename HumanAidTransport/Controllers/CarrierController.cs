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
                // Check if carrier with the same name already exists
                bool carrierExists = _context.Carriers.Any(c => c.Name == carrier.Name);
                if (carrierExists)
                {
                    ModelState.AddModelError("", "A carrier with this name already exists.");
                }

                // Check if car number already exists
                bool vehicleNumberExists = _context.Carriers.Any(c => c.VehicleNumber == carrier.VehicleNumber);
                if (vehicleNumberExists)
                {
                    ModelState.AddModelError("", "A carrier with this vehicle number already exists.");
                }

                // Check if phone number (Contacts) already exists
                bool phoneNumberExists = _context.Carriers.Any(c => c.Contacts == carrier.Contacts);
                if (phoneNumberExists)
                {
                    ModelState.AddModelError("", "A carrier with this phone number already exists.");
                }

                // If no errors, save the new carrier
                if (!carrierExists && !vehicleNumberExists && !phoneNumberExists)
                {
                    try
                    {
                        _context.Carriers.Add(carrier);
                        _context.SaveChanges();
                        return RedirectToAction("Index", "Home");
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

            var carrier = _context.Carriers.FirstOrDefault(c => c.Name == name && c.Password == password);

            if (carrier != null)
            {
                //ProfileController.Carrier = carrier;
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Incorrect username or password for the carrier.");
                return View("~/Views/Login/CarrierLogin.cshtml");
            }
        }
    }
}
