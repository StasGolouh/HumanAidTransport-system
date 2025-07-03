using HumanitarianTransport.Data;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HumanAidTransport.Controllers
{
    public class CarrierProfileController : Controller
    {
        public static Carrier? Carrier;

        private readonly HumanitarianDbContext _context;

        public CarrierProfileController(HumanitarianDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> CarrierProfile()
        {
            if (Carrier != null)
            {
                // Завантажуємо доступні завдання зі статусами "Новий" або "В очікуванні"
                var availableTasks = await _context.Volunteers
                    .SelectMany(v => v.Tasks)
                    .Where(t => t.Status == "Новий" || t.Status == "В очікуванні")
                    .ToListAsync();

                // Завантажуємо перевізника та його завдання
                var carrierWithTasks = await _context.Carriers
                    .Include(c => c.AvailableTasks)
                    .Include(c => c.Ratings)
                    .FirstOrDefaultAsync(c => c.Id == Carrier.Id);

              
                // Якщо перевізник знайдений
                if (carrierWithTasks != null)
                {
                    // Додаємо доступні завдання
                    carrierWithTasks.AvailableTasks.AddRange(availableTasks);

                    // Отримуємо всі замовлення для цих запитів
                    var orders = await _context.TransportOrders
                        .Where(o =>o.CarrierId == Carrier.Id)
                        .ToListAsync();

                    // Підрахунок нових завдань
                    int newOrderCount = orders.Count(t => t.Status == "Новий");

                    // Підрахунок нових сповіщень
                    int newNotificationsCount = await _context.Notifications
                        .Where(n => n.CarrierId == Carrier.Id && (n.Status == "Підтверджено" || n.Status == "Скасовано" || n.Status == "Оцінено" || n.Status == "Оплачено" || n.Status == "Штраф Перевізнику" || n.Status == "Компенсація Перевізнику"))
                        .CountAsync();

                    // Передаємо кількість нових завдань та сповіщень у View
                    ViewBag.NewTasksCount = newOrderCount;
                    ViewBag.NewNotificationsCount = newNotificationsCount;

                    return View("~/Views/Profile/CarrierProfile.cshtml", carrierWithTasks);
                }
            }

            return RedirectToAction("CarrierLogin", "Carrier");
        }

        [HttpPost]
        public async Task<IActionResult> CarrierAddBalance(int amountToAdd, string selectedCard)
        {
            if (Carrier == null)
            {
                TempData["ErrorMessage"] = "Ви не авторизовані";
                return RedirectToAction("CarrierLogin", "Carrier");
            }

            if (amountToAdd <= 0)
            {
                TempData["ErrorMessage"] = "Сума для додавання має бути більшою за 0";
                return RedirectToAction("CarrierProfile");
            }

            var carrierFromDb = await _context.Carriers.FirstOrDefaultAsync(c => c.Id == Carrier.Id);

            if (carrierFromDb == null)
            {
                TempData["ErrorMessage"] = "Перевізника не знайдено";
                return RedirectToAction("CarrierProfile");
            }

            // Перевірка, що вибрана картка належить перевізнику
            if (selectedCard != carrierFromDb.CardNumber)
            {
                TempData["ErrorMessage"] = "Оберіть коректну картку";
                return RedirectToAction("CarrierProfile");
            }

            carrierFromDb.Balance += amountToAdd;
            _context.Carriers.Update(carrierFromDb);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Баланс успішно поповнено на {amountToAdd} грн.";
            return RedirectToAction("CarrierProfile");
        }

        [HttpPost]
        public async Task<IActionResult> UploadPhoto(IFormFile profilePhoto)
        {
            var carrier = await _context.Carriers.FirstOrDefaultAsync(c=> c.Id == CarrierProfileController.Carrier.Id);

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
            carrier.ProfilePhotoURL = $"/images/profile_photos/{fileName}";
            _context.Carriers.Update(carrier);
            await _context.SaveChangesAsync();

            return Json(new { success = true, imageUrl = carrier.ProfilePhotoURL });
        }

        public IActionResult LogOut()
        {
            Carrier = null;
            return RedirectToAction("Index", "Home");
        }
    }
}
