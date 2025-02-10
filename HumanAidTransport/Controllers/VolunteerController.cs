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
            if (ModelState.IsValid)
            {
                // Перевіряємо, чи вже існує волонтер з таким ім'ям
                bool volunteerExists = _context.Volunteers.Any(v => v.Name == volunteer.Name);

                if (!volunteerExists)
                {
                    try
                    {
                        // Додаємо волонтера в базу
                        _context.Volunteers.Add(volunteer);
                        _context.SaveChanges();

                        return RedirectToAction("VolunteerProfile", "VolunProfile");
                    }
                    catch (Exception ex)
                    {
                        // Логування помилки
                        ModelState.AddModelError("", "Error saving volunteer: " + ex.Message);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "A customer with this name already exists.");
                }
            }

            // Повертаємо з помилками на форму
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

            var volunteer = _context.Volunteers.FirstOrDefault(c => c.Name == name && c.Password == password);

            if (volunteer != null)
            {
                VolunProfileController.Volunteer = volunteer;
                HttpContext.Session.SetString("UserName", name);
                return RedirectToAction("VolunteerProfile", "VolunProfile");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Incorrect username or password for the carrier.");
                return View("~/Views/Login/VolunteerLogin.cshtml");
            }
        }


    }
}
