using System.Security.Claims;
using HumanAidTransport.Models;
using HumanitarianTransport.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HumanAidTransport.Controllers
{
    public class VolunProfileController : Controller
    {
        public static Volunteer? Volunteer;

        private readonly HumanitarianDbContext _context;

        public VolunProfileController(HumanitarianDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> VolunteerProfile()
        {
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
                // Завантажуємо волонтера 
                var volunteerFromDb = await _context.Volunteers
                    .Include(v => v.Tasks)
                    .FirstOrDefaultAsync(v => v.Id == Volunteer.Id);

                if (volunteerFromDb != null)
                {
                    // Додаємо нове завдання до волонтера
                    volunteerFromDb.Tasks.Add(newTask);
                    _context.HumanitarianAids.Add(newTask);  // Додаємо нове завдання в бд
                    await _context.SaveChangesAsync();  // Збереження змін

                    return RedirectToAction("VolunteerProfile");
                }
            }

            return View("VolunteerProfile");
        }

        [HttpPost]
        public async Task<IActionResult> UploadPhoto(IFormFile profilePhoto)
        {
            if (profilePhoto == null || profilePhoto.Length == 0)
                return Json(new { success = false, message = "File is not chosen" });

            if (VolunProfileController.Volunteer == null)
                return Unauthorized();

            var volunteer = await _context.Volunteers.FirstOrDefaultAsync(v => v.Id == VolunProfileController.Volunteer.Id);
            if (volunteer == null)
                return NotFound();

            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profile_photos");
            string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(profilePhoto.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await profilePhoto.CopyToAsync(fileStream);
            }

            volunteer.ProfilePhotoURL = "/images/profile_photos/" + uniqueFileName;
            _context.Volunteers.Update(volunteer);
            await _context.SaveChangesAsync();

            return Json(new { success = true, imageUrl = "/images/profile_photos/" + uniqueFileName });
        }

        public IActionResult LogOut()
        {
            Volunteer = null;
            return RedirectToAction("Index", "Home");
        }
    }
}
