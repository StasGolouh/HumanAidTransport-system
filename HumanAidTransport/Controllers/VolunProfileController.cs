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
                // Завантажуємо волонтера з бази даних
                var volunteerFromDb = await _context.Volunteers
                    .Include(v => v.Tasks)
                    .FirstOrDefaultAsync(v => v.Id == Volunteer.Id);

                if (volunteerFromDb != null)
                {
                    newTask.VolunteerId = volunteerFromDb.Id; 
                    volunteerFromDb.Tasks.Add(newTask);  

                    _context.HumanitarianAids.Add(newTask);  
                    await _context.SaveChangesAsync(); 

                    return RedirectToAction("VolunteerProfile"); 
                }
            }

            return View("VolunteerProfile");  
        }

        [HttpPost]
        public async Task<IActionResult> CancelTask(int taskId)
        {
            if (Volunteer != null)
            {
                // Завантажуємо волонтера і його завдання
                var volunteerFromDb = await _context.Volunteers
                    .Include(v => v.Tasks)
                    .FirstOrDefaultAsync(v => v.Id == Volunteer.Id);

                if (volunteerFromDb != null)
                {
                    var taskToCancel = volunteerFromDb.Tasks.FirstOrDefault(t => t.HumanAidId == taskId);
                    if (taskToCancel != null)
                    {
                        // Видалення завдання
                        volunteerFromDb.Tasks.Remove(taskToCancel);
                        _context.HumanitarianAids.Remove(taskToCancel);  // Видалення з бази даних
                        await _context.SaveChangesAsync();

                        return RedirectToAction("VolunteerProfile");  
                    }
                }
            }

            return RedirectToAction("VolunteerLogin", "Volunteer");
        }

        [HttpPost]
        public async Task<IActionResult> UploadPhoto(IFormFile profilePhoto)
        {
            var volunteer = await _context.Volunteers.FirstOrDefaultAsync(v => v.Id == VolunProfileController.Volunteer.Id);

            // Шлях до папки
            string uploadsFolder = Path.Combine("wwwroot", "images", "profile_photos");

            // Створюємо папку, якщо її немає
            Directory.CreateDirectory(uploadsFolder);

            // Отримуємо список файлів формату photoX.jpg
            var existingFiles = Directory.GetFiles(uploadsFolder, "photo*.jpg");

            // Знаходимо найбільший номер фото
            int nextNumber = existingFiles.Length + 1;
            string fileName = $"photo{nextNumber}.jpg";
            string filePath = Path.Combine(uploadsFolder, fileName);

            // Зберігаємо файл
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await profilePhoto.CopyToAsync(fileStream);
            }

            // Оновлюємо шлях у базі даних
            volunteer.ProfilePhotoURL = $"/images/profile_photos/{fileName}";
            _context.Volunteers.Update(volunteer);
            await _context.SaveChangesAsync();

            return Json(new { success = true, imageUrl = volunteer.ProfilePhotoURL });
        }

        public IActionResult LogOut()
        {
            Volunteer = null;
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> VolunteerRequestList()
        {
            return View("~/Views/Notification/VolunteerRequestList.cshtml");
        }
    }
}
