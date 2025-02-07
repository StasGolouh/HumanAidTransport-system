using HumanAidTransport.Models;
using HumanitarianTransport.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HumanAidTransport.Controllers
{
    public class ProfileController : Controller
    {
        public static Volunteer? Volunteer;

        private readonly HumanitarianDbContext _context;

        public ProfileController(HumanitarianDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> VolunteerProfile()
        {
            // Перевірка на наявність волонтера
            if (Volunteer != null)
            {
                // Завантаження волонтера з бази даних і його завдань
                var volunteerFromDb = await _context.Volunteers
                    .Include(v => v.Tasks) // Завантаження завдань волонтера
                    .FirstOrDefaultAsync(v => v.Id == Volunteer.Id);

                if (volunteerFromDb != null)
                {
                    return View("~/Views/Profile/VolunteerProfile.cshtml", volunteerFromDb);
                }
            }

            return RedirectToAction("VolunteerLogin", "Volunteer");
        }

        [HttpPost]
        public async Task<IActionResult> PostTask(HumanitarianAid newTask)
        {
            if (ModelState.IsValid && Volunteer != null)
            {
                // Завантаження волонтера з бази даних
                var volunteerFromDb = await _context.Volunteers
                    .Include(v => v.Tasks)
                    .FirstOrDefaultAsync(v => v.Id == Volunteer.Id);

                if (volunteerFromDb != null)
                {
                    // Додавання нового завдання до волонтера
                    volunteerFromDb.Tasks.Add(newTask);
                    _context.HumanitarianAids.Add(newTask);  // Додавання нового завдання в базу даних
                    await _context.SaveChangesAsync();  // Збереження змін

                    return RedirectToAction("VolunteerProfile");
                }
            }

            return View("VolunteerProfile");
        }

        public IActionResult LogOut()
        {
            Volunteer = null;
            return RedirectToAction("Index", "Home");
        }
    }
}
