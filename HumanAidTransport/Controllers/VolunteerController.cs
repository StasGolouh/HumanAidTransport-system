using HumanitarianTransport.Data;
using Microsoft.AspNetCore.Mvc;


namespace HumanAidTransport.Controllers
{
    public class VolunteerController : Controller
    {
        private readonly HumanitarianDbContext _context;

        public VolunteerController(HumanitarianDbContext context)
        {
            _context = context;
        }

        public IActionResult VolunteerRegistration()
        {
            return View("~/Views/Registration/VolunteerRegistration.cshtml");
        }

        [HttpPost]
        public IActionResult VolunteerRegistration(Volunteer volunteer)
        {
            // Перевірка імені — літери, цифри, пробіли, мін. 2 символи
            if (!System.Text.RegularExpressions.Regex.IsMatch(volunteer.Name ?? "", @"^[a-zA-Zа-яА-ЯіІїЇєЄґҐ0-9\s]{2,}$"))
            {
                ModelState.AddModelError("Name", "Name can contain letters, numbers, and spaces only (min 2 characters).");
            }

            // Перевірка пароля — мін. 8 символів, без пробілів
            if (string.IsNullOrWhiteSpace(volunteer.Password) || volunteer.Password.Length < 8 || volunteer.Password.Contains(" "))
            {
                ModelState.AddModelError("Password", "Password must be at least 8 characters long and contain no spaces.");
            }

            if (ModelState.IsValid)
            {
                bool volunteerExists = _context.Volunteers.Any(v => v.Name == volunteer.Name);

                if (!volunteerExists)
                {
                    try
                    {
                        _context.Volunteers.Add(volunteer);
                        _context.SaveChanges();

                        TempData["RegistMessage"] = "Registration was successful, please log in.";
                        return RedirectToAction("VolunteerProfile", "VolunProfile");
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Error saving volunteer: " + ex.Message);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "A volunteer with this name already exists.");
                }
            }

            return View("~/Views/Registration/VolunteerRegistration.cshtml");
        }


        //=============================Login====================================

        public IActionResult VolunteerLogin()
        {
            return View("~/Views/Login/VolunteerLogin.cshtml");
        }

        [HttpPost]
        public IActionResult VolunteerLogin(string name, string password)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError(string.Empty, "Please fill in all fields.");
                return View("~/Views/Login/VolunteerLogin.cshtml");
            }

            // Валідація імені
            if (!System.Text.RegularExpressions.Regex.IsMatch(name ?? "", @"^[a-zA-Zа-яА-ЯіІїЇєЄґҐ0-9\s]{2,}$"))
            {
                ModelState.AddModelError("name", "Name can contain letters, numbers, and spaces only (min 2 characters).");
                return View("~/Views/Login/VolunteerLogin.cshtml");
            }

            // Валідація пароля
            if (password.Length < 8 || password.Contains(" "))
            {
                ModelState.AddModelError("password", "Password must be at least 8 characters and contain no spaces.");
                return View("~/Views/Login/VolunteerLogin.cshtml");
            }

            var volunteer = _context.Volunteers.FirstOrDefault(c => c.Name == name && c.Password == password);

            if (volunteer != null)
            {
                VolunProfileController.Volunteer = volunteer;
                HttpContext.Session.SetString("UserName", name);
                return RedirectToAction("VolunteerProfile", "VolunProfile");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Incorrect username or password for the volunteer.");
                return View("~/Views/Login/VolunteerLogin.cshtml");
            }
        }
    }
}
