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
            // Ensure you have the necessary data for the volunteer
            if (Volunteer != null)
            {
                return View("~/Views/Profile/VolunteerProfile.cshtml", Volunteer);
            }

            return RedirectToAction("VolunteerLogin", "Volunteer");
        }


        public IActionResult LogOut()
        {
            Volunteer = null;
            return RedirectToAction("Index", "Home");
        }
    }
}
