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
                    .Include(v => v.Tasks)
                    .FirstOrDefaultAsync(v => v.Id == Volunteer.Id);

                if (volunteerFromDb != null)
                {
                    var acceptedTasks = volunteerFromDb.Tasks.ToList();
                    volunteerFromDb.Tasks = acceptedTasks;

                    //Лічильник запитів
                    var deliveryRequest = await _context.DeliveryRequests
                        .Where(r => r.VolunteerId == volunteerFromDb.Id)
                        .ToListAsync(); 

                    int newRequestCount = deliveryRequest.Count();
                    ViewBag.NewRequestCount = newRequestCount;


                    //Лічильник сповіщень
                    int newNotificationsCount = await _context.Notifications
                      .Where(n => n.VolunteerId == Volunteer.Id && (n.Status == "Completed" || n.Status == "Rejected" || n.Status == "In progress"))
                      .CountAsync();

                    ViewBag.NewNotificationsCount = newNotificationsCount;


                    return View("~/Views/Profile/VolunteerProfile.cshtml", volunteerFromDb);
                }
            }

            return RedirectToAction("VolunteerLogin", "Volunteer");
        }

        [HttpPost]
        public async Task<IActionResult> PostTask(HumanitarianAid newTask)
        {
            if (Volunteer == null)
            {
                TempData["Error"] = "You are not authorized!";
                return RedirectToAction("VolunteerLogin", "Volunteer");
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please check the entered data!";
                return RedirectToAction("VolunteerProfile");
            }

            if (newTask.Quantity <= 0)
            {
                TempData["Error"] = "Quantity must be greater than 0!";
                return RedirectToAction("VolunteerProfile");
            }

            if (newTask.Payment < 0)
            {
                TempData["Error"] = "Payment cannot be negative!";
                return RedirectToAction("VolunteerProfile");
            }

            if (string.IsNullOrWhiteSpace(newTask.DeliveryAddressFrom) || string.IsNullOrWhiteSpace(newTask.DeliveryAddressTo))
            {
                TempData["Error"] = "Delivery addresses cannot be empty!";
                return RedirectToAction("VolunteerProfile");
            }

            if (newTask.ExpectedDeliveryTime  <= DateTime.Now)
            {
                TempData["Error"] = "The delivery date must be in the future!";
                return RedirectToAction("VolunteerProfile");
            }

            var volunteerFromDb = await _context.Volunteers
                .Include(v => v.Tasks)
                .FirstOrDefaultAsync(v => v.Id == Volunteer.Id);

            if (volunteerFromDb == null)
            {
                TempData["Error"] = "Volunteer not found!";
                return RedirectToAction("VolunteerProfile");
            }

            newTask.VolunteerId = volunteerFromDb.Id;
            newTask.Status = "New";
            volunteerFromDb.Tasks.Add(newTask);
            _context.HumanitarianAids.Add(newTask);
            await _context.SaveChangesAsync();

            TempData["SuccessVol"] = "The task has been successfully added!";
            return RedirectToAction("VolunteerProfile");
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
                        // Видаляємо всі пов'язані заявки
                        var deliveryRequests = await _context.DeliveryRequests
                            .Where(dr => dr.HumanAidId == taskToCancel.HumanAidId)
                            .ToListAsync();
                        _context.DeliveryRequests.RemoveRange(deliveryRequests);

                        // Оновлюємо статус завдання
                        taskToCancel.Status = "Canceled";

                        // Зберігаємо зміни в базі даних
                        await _context.SaveChangesAsync();

                        TempData["CanceledMess"] = $"Task {taskToCancel.Name} has been successfully canceled.";
                        return RedirectToAction("VolunteerProfile");
                    }
                }
            }

            return RedirectToAction("VolunteerLogin", "Volunteer");
        }

        [HttpPost]
        public async Task<IActionResult> RestoreTask(int taskId)
        {
            if (Volunteer != null)
            {
                // Завантажуємо волонтера і його завдання
                var volunteerFromDb = await _context.Volunteers
                    .Include(v => v.Tasks)
                    .FirstOrDefaultAsync(v => v.Id == Volunteer.Id);

                if (volunteerFromDb != null)
                {
                    var taskToRestore = volunteerFromDb.Tasks.FirstOrDefault(t => t.HumanAidId == taskId);
                    if (taskToRestore != null)
                    {
                        // Відновлюємо статус завдання
                        taskToRestore.Status = "New";

                        // Зберігаємо зміни в базі даних
                        await _context.SaveChangesAsync();

                        TempData["SuccessVol"] = $"Task { taskToRestore.Name } has been successfully restored.";
                        return RedirectToAction("VolunteerProfile");
                    }
                }
            }

            return RedirectToAction("VolunteerLogin", "Volunteer");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTask(int taskId)
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
                        // Видаляємо всі пов'язані заявки
                        var deliveryRequests = await _context.DeliveryRequests
                            .Where(dr => dr.HumanAidId == taskToCancel.HumanAidId)
                            .ToListAsync();
                        _context.DeliveryRequests.RemoveRange(deliveryRequests);

                        // Видаляємо завдання
                        volunteerFromDb.Tasks.Remove(taskToCancel);

                        // Зберігаємо зміни в базі даних
                        await _context.SaveChangesAsync();

                        TempData["DeleteMess"] = $"Task {taskToCancel.Name} has been successfully deleted.";
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

        public async Task<IActionResult> VolunteerRequestList(int volunteerId)
        {
            var volunteer = await _context.Volunteers
                .FirstOrDefaultAsync(v => v.Id == volunteerId);

            var requests = await _context.DeliveryRequests
                .Where(r => r.VolunteerId == volunteerId)
                .Include(r => r.HumanitarianAid)  
                .Include(r => r.Carrier)  
                .ToListAsync();

            // Повертаємо дані до вьюхи
            return View("~/Views/Lists/VolunteerRequestList.cshtml", requests);
        }

        public IActionResult LogOut()
        {
            Volunteer = null;
            return RedirectToAction("Index", "Home");
        }
    }
}
