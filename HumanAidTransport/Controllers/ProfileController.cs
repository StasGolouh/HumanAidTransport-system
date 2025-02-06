using HumanitarianTransport.Data;
using Microsoft.AspNetCore.Mvc;


namespace HumanAidTransport.Controllers
{
    public class ProfileController : Controller
    {
        public static Carrier? Carrier;
        public static Volunteer? Volunteer;

        private readonly HumanitarianDbContext _context;

        public ProfileController(HumanitarianDbContext context)
        {
            _context = context;
        }

        public IActionResult VolunteerProfile()
        {
            return View("~/Views/Profile/VolunteerProfile.cshtml");
        }


        public IActionResult LogOut()
        {
            Carrier = null;
            return RedirectToAction("Index", "Home");
        }
    }
}
