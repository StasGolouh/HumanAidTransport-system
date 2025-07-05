using HumanAidTransport.Models;
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
                await CheckOverOrders(Carrier.Id);
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

        private async Task CheckOverOrders(int carrierId)
        {
            var utcNow = DateTime.UtcNow;
            var ukraineTimeZone = TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time"); 
            var now = TimeZoneInfo.ConvertTimeFromUtc(utcNow, ukraineTimeZone);

            var overdueOrders = await _context.TransportOrders
                .Where(orders => orders.CarrierId == carrierId
                             && orders.Status != "Виконано"
                             && orders.Status != "Прострочено"
                             && orders.ExpectedDeliveryTime <= now.AddMinutes(-1))
                .ToListAsync();

            var carrier = await _context.Carriers.FirstOrDefaultAsync(c => c.Id == carrierId);

            foreach (var order in overdueOrders)
            {
                var task = await _context.HumanitarianAids.FirstOrDefaultAsync(aids => aids.HumanAidId == order.HumanAidId);

                if (task.Status != "Прострочено") task.Status = "Прострочено";

                if (order.Status != "Прострочено") order.Status = "Прострочено";

                double penaltyAmount = task.Payment.Value * 0.5;

                double debt = 0;

                string carrierNotificationMessage;

                if (carrier.Balance >= penaltyAmount)
                {
                    carrier.Balance -= penaltyAmount;
                    carrierNotificationMessage =
                        $"Доставка завдання \"{task.Name}\" прострочена більш ніж на 6 годин. Штраф {penaltyAmount} грн.";
                }
                else
                {
                    debt = penaltyAmount - carrier.Balance;
                    carrier.Balance = 0;
                    carrier.Debt += debt;

                    carrierNotificationMessage =
                        $"Доставка завдання \"{task.Name}\" прострочена більш ніж на 6 годин. Частково списано {penaltyAmount} грн, залишок боргу {debt} грн.";
                }

                carrier.ViolationsCount++;

                // Сповіщення перевізнику
                _context.Notifications.Add(new Notification
                {
                    CarrierId = carrier.Id,
                    VolunteerId = task.VolunteerId,
                    Message = carrierNotificationMessage,
                    CreatedAt = now,
                    Status = "Штраф Перевізнику"
                });

                // Компенсація волонтеру
                var volunteer = await _context.Volunteers.FirstOrDefaultAsync(v => v.Id == task.VolunteerId);
                if (volunteer != null)
                {
                    volunteer.Balance += penaltyAmount;

                    _context.Notifications.Add(new Notification
                    {
                        VolunteerId = volunteer.Id,
                        CarrierId = carrier.Id,
                        Message =
                            $"Вам нараховано компенсацію {penaltyAmount} грн за прострочену доставку завдання \"{task.Name}\" перевізником.",
                        CreatedAt = now,
                        Status = "Компенсація Волонтеру"
                    });

                    _context.Volunteers.Update(volunteer);
                }

                _context.HumanitarianAids.Update(task);
            }

            _context.Carriers.Update(carrier);
            await _context.SaveChangesAsync();
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

            if (selectedCard != carrierFromDb.CardNumber)
            {
                TempData["ErrorMessage"] = "Оберіть коректну картку";
                return RedirectToAction("CarrierProfile");
            }

            double remainingAmount = amountToAdd;

            if (carrierFromDb.Debt > 0)
            {
                if (remainingAmount >= carrierFromDb.Debt)
                {
                    remainingAmount -= carrierFromDb.Debt;
                    TempData["SuccessMessage"] = $"Борг у розмірі {carrierFromDb.Debt} грн погашено.";
                    carrierFromDb.Debt = 0;
                }
                else
                {
                    carrierFromDb.Debt -= remainingAmount;
                    TempData["SuccessMessage"] = $"Частково погашено борг на {amountToAdd} грн. Залишок боргу: {carrierFromDb.Debt} грн.";
                    remainingAmount = 0;
                }
            }

            carrierFromDb.Balance += remainingAmount;

            _context.Carriers.Update(carrierFromDb);
            await _context.SaveChangesAsync();

            if (remainingAmount > 0)
            {
                TempData["SuccessMessage"] += $" Баланс поповнено на {remainingAmount} грн.";
            }

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
