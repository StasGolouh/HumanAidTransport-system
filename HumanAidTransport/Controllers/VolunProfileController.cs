using HumanAidTransport.Models;
using HumanitarianTransport.Data;
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

        // Алгоритм розподілу гуманітарних вантажів, враховуєчи терміновість, критичність(дата) та тип 
        private Priority CalculatePriorityScore(HumanitarianAid task)
        {
            // Множник типу допомоги
            int aidWeight = task.Type switch
            {
                AidType.Military => 3,
                AidType.Shelter => 2,
                AidType.Medicine => 3,
                AidType.Food => 2,
                AidType.Clothes => 1,
                AidType.Other => 1,
                _ => 1
            };

            // Години до доставки
            double hoursUntilDelivery = task.ExpectedDeliveryTime.HasValue
                ? (task.ExpectedDeliveryTime.Value - DateTime.Now).TotalHours
                : 24;

            // Базовий рівень критичності на основі типу допомоги
            Priority mainPriority;

            if (aidWeight == 3)
                mainPriority = Priority.High;
            else if (aidWeight == 2)
                mainPriority = Priority.Medium;
            else
                mainPriority = Priority.Low;

            // Підвищення рівня критичності на основі терміновості
            if (hoursUntilDelivery <= 48)
            {
                if (mainPriority == Priority.Low)
                    return Priority.Medium;
                else
                    return Priority.High;
            }

            return mainPriority;
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
                      .Where(n => n.VolunteerId == Volunteer.Id && (n.Status == "Виконано" || n.Status == "Відхилено" || n.Status == "В процесі"))
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
                TempData["Error"] = "Ви не авторизовані";
                return RedirectToAction("VolunteerLogin", "Volunteer");
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Будь ласка, перевірте введені дані";
                return RedirectToAction("VolunteerProfile");
            }

            if (newTask.Quantity <= 0)
            {
                TempData["Error"] = "Кількість має бути більше 0";
                return RedirectToAction("VolunteerProfile");
            }

            var random = new Random();
            newTask.PinCode = random.Next(100000, 999999);

            if (newTask.Payment < 0)
            {
                TempData["Error"] = "Оплата не може бути негативною";
                return RedirectToAction("VolunteerProfile");
            }

            if (string.IsNullOrWhiteSpace(newTask.DeliveryAddressFrom) || string.IsNullOrWhiteSpace(newTask.DeliveryAddressTo))
            {
                TempData["Error"] = "Адреси доставки не можуть бути пустими";
                return RedirectToAction("VolunteerProfile");
            }

            if (newTask.ExpectedDeliveryTime  <= DateTime.Now)
            {
                TempData["Error"] = "Дата доставки має бути в майбутньому";
                return RedirectToAction("VolunteerProfile");
            }

            var volunteerFromDb = await _context.Volunteers
                .Include(v => v.Tasks)
                .FirstOrDefaultAsync(v => v.Id == Volunteer.Id);

            if (volunteerFromDb == null)
            {
                TempData["Error"] = "Волонтера не знайдено";
                return RedirectToAction("VolunteerProfile");
            }

            if (volunteerFromDb.Balance < newTask.Payment)
            {
                TempData["Error"] = "Недостатньо коштів, щоб розплатитися за завдання. Поповніть баланс або зменшіть ціну за доставку.";
                return RedirectToAction("VolunteerProfile");
            }

            newTask.VolunteerId = volunteerFromDb.Id;
            newTask.Status = "Новий";

            // Перерахунок пріоритету для нового завдання
            newTask.PriorityLevel = CalculatePriorityScore(newTask);
            volunteerFromDb.Tasks.Add(newTask);
            _context.HumanitarianAids.Add(newTask);
            await _context.SaveChangesAsync();

            TempData["SuccessVol"] = "Завдання успішно додано";
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
                        taskToCancel.Status = "Скасовано";

                        // Зберігаємо зміни в базі даних
                        await _context.SaveChangesAsync();

                        TempData["CanceledMess"] = $"Завдання {taskToCancel.Name} було успішно скасовано.";
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
                        taskToRestore.Status = "Новий";

                        // Зберігаємо зміни в базі даних
                        await _context.SaveChangesAsync();

                        TempData["SuccessVol"] = $"Завдання { taskToRestore.Name } було успішно відновлено.";
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

                        TempData["DeleteMess"] = $"Завдання {taskToCancel.Name} було успішно видалено.";
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

        [HttpPost]
        public async Task<IActionResult> VolAddBalance(int amountToAdd, string selectedCard)
        {
            if (Volunteer == null)
            {
                TempData["Error"] = "Ви не авторизовані";
                return RedirectToAction("VolunteerLogin", "Volunteer");
            }

            if (amountToAdd <= 0)
            {
                TempData["Error"] = "Сума для додавання має бути більшою за 0";
                return RedirectToAction("VolunteerProfile");
            }

            var volunteerFromDb = await _context.Volunteers.FirstOrDefaultAsync(v => v.Id == Volunteer.Id);

            if (volunteerFromDb == null)
            {
                TempData["Error"] = "Волонтера не знайдено";
                return RedirectToAction("VolunteerProfile");
            }

            // Тут ти можеш перевірити чи selectedCard дійсно належить волонтеру (для безпеки)
            if (selectedCard != volunteerFromDb.CardNumber)
            {
                TempData["Error"] = "Оберіть коректну картку";
                return RedirectToAction("VolunteerProfile");
            }

            volunteerFromDb.Balance += amountToAdd;
            _context.Volunteers.Update(volunteerFromDb);
            await _context.SaveChangesAsync();

            TempData["SuccessVol"] = $"Баланс успішно поповнено на {amountToAdd} грн.";
            return RedirectToAction("VolunteerProfile");
        }

        [HttpPost]
        public IActionResult PayForDelivery(int taskId)
        {
            var task = _context.HumanitarianAids.FirstOrDefault(t => t.HumanAidId == taskId);

            if (task == null)
            {
                TempData["Error"] = "Завдання не знайдено.";
                return RedirectToAction("VolunteerProfile");
            }

            var volunteer = _context.Volunteers.FirstOrDefault(v => v.Id == task.VolunteerId);
            if (volunteer == null)
            {
                TempData["Error"] = "Волонтера не знайдено.";
                return RedirectToAction("VolunteerProfile");
            }

            if (volunteer.Balance < task.Payment)
            {
                TempData["Error"] = "Недостатньо коштів для оплати доставки.";
                return RedirectToAction("VolunteerProfile");
            }

            // Знаходимо delivery request, який стосується цього завдання
            var deliveryRequest = _context.DeliveryRequests
                .FirstOrDefault(dr => dr.HumanAidId == task.HumanAidId);

            if (deliveryRequest == null || deliveryRequest.CarrierId == 0)
            {
                TempData["Error"] = "Перевізника для цього завдання не знайдено.";
                return RedirectToAction("VolunteerProfile");
            }

            var carrier = _context.Carriers.FirstOrDefault(c => c.Id == deliveryRequest.CarrierId);
            if (carrier == null)
            {
                TempData["Error"] = "Перевізника не знайдено.";
                return RedirectToAction("VolunteerProfile");
            }

            // списання коштів з волонтера
            volunteer.Balance -= task.Payment.Value;

            // нарахування коштів перевізнику
            carrier.Balance += task.Payment.Value;

            // оновлення статусу, наприклад
            task.Status = "Оплачено";

            _context.SaveChanges();

            TempData["SuccessVol"] = "Доставка успішно оплачена!";
            return RedirectToAction("VolunteerProfile");
        }


        public IActionResult LogOut()
        {
            Volunteer = null;
            return RedirectToAction("Index", "Home");
        }
    }
}
