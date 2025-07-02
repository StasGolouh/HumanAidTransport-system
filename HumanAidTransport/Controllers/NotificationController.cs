using HumanAidTransport.Models;
using HumanitarianTransport.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HumanAidTransport.Controllers
{
    public class NotificationController : Controller
    {
        private readonly HumanitarianDbContext _context;

        public NotificationController(HumanitarianDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> VolunteerNotifications(int volunteerId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.VolunteerId == volunteerId && (n.Status == "Виконано" || n.Status == "Відхилено" || n.Status =="В процесі"))
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return View("~/Views/Notification/VolunteerNotifications.cshtml", notifications);
        }

        public async Task<IActionResult> CarrierNotifications(int carrierId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.CarrierId == carrierId && (n.Status == "Підтверджено" || n.Status == "Скасовано" || n.Status == "Оцінено" || n.Status == "Оплачено"))
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return View("~/Views/Notification/CarrierNotifications.cshtml", notifications);
        }

        [HttpPost]
        public async Task<IActionResult> VolDelete(int notificationId, int volunteerId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("VolunteerNotifications", new { volunteerId });
        }


        [HttpPost]
        public async Task<IActionResult> CarrDelete(int notificationId, int carrierId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("CarrierNotifications", new { carrierId });
        }

        [HttpPost]
        public async Task<IActionResult> RateCarrier(int notificationId, int rating)
        {
            // Знаходимо notification по id
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId);

            if (notification == null)
            {
                return NotFound("Сповіщення не знайдено.");
            }

            var carrier = await _context.Carriers
                .FirstOrDefaultAsync(c => c.Id == notification.CarrierId);

            if (carrier == null)
            {
                return NotFound("Перевізник не знайдено.");
            }

            // Перевірка, чи вже є оцінка для цього перевізника та цього notification
            var existingRating = await _context.CarrierRatings
                .FirstOrDefaultAsync(r => r.CarrierId == carrier.Id && r.NotificationId == notificationId);

            if (existingRating != null)
            {
                TempData["ErrorMessage"] = "Ви вже оцінили цього перевізника для цього завдання.";
                return RedirectToAction("VolunteerNotifications", new { volunteerId = notification.VolunteerId });
            }

            // Створюємо новий рейтинг
            var carrierRating = new CarrierRating
            {
                CarrierId = carrier.Id,
                Rating = rating,
                NotificationId = notificationId
            };

            // Додаємо новий рейтинг в таблицю CarrierRatings
            _context.CarrierRatings.Add(carrierRating);

            // Зберігаємо зміни в базі даних
            await _context.SaveChangesAsync();

            // Додаємо рейтинг в список Ratings перевізника
            carrier.Ratings.Add(carrierRating);

            // Оновлюємо перевізника в базі
            _context.Carriers.Update(carrier);

            var notifications = new Notification
            {
                VolunteerId = notification.VolunteerId,
                CarrierId = carrier.Id,
                Message = $"Вашу роботу оцінили {carrierRating.Rating}. ",
                CreatedAt = DateTime.UtcNow,
                Status = "Оцінено"

            };
            _context.Notifications.Add(notifications);

            await _context.SaveChangesAsync();

            TempData["TrueMessage"] = "Рейтинг успішно подано!";
            return RedirectToAction("VolunteerNotifications", new { volunteerId = notification.VolunteerId });
        }
    }
}

