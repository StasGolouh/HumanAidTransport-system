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
                bool carrierExists = _context.Carriers.Any(c => c.Name == carrier.Name);

                if (!carrierExists)
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
                else
                {
                    ModelState.AddModelError("", "A carrier with this name already exists.");
                }
            }
            return View(carrier);
        }

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
                return View();
            }

            var carrier = _context.Carriers.FirstOrDefault(c => c.Name == name && c.Password == password);

            if (carrier != null)
            {
                ProfileController.Carrier = carrier;
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Incorrect username or password for the carrier.");
                return View();
            }
        }
    }
}
