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
                .Where(n => n.VolunteerId == volunteerId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return View("~/Views/Notification/VolunteerNotifications.cshtml", notifications);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int notificationId, int volunteerId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                _context.Update(notification);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(VolunteerNotifications), new { volunteerId });
        }

        [HttpPost]
        public async Task<IActionResult> RateCarrier(int notificationId, int rating)
        {
            // Знаходимо notification по id
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId);

            if (notification == null)
            {
                return NotFound("Notification not found.");
            }

            var carrier = await _context.Carriers
                .FirstOrDefaultAsync(c => c.Id == notification.CarrierId);

            if (carrier == null)
            {
                return NotFound("Carrier not found.");
            }

            // Перевірка, чи вже є оцінка для цього перевізника та цього notification
            var existingRating = await _context.CarrierRatings
                .FirstOrDefaultAsync(r => r.CarrierId == carrier.Id && r.NotificationId == notificationId);

            if (existingRating != null)
            {
                TempData["ErrorMessage"] = "You have already rated this carrier for this task.";
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
            await _context.SaveChangesAsync();

            TempData["TrueMessage"] = "Rating has been successfully submitted!";
            return RedirectToAction("VolunteerNotifications", new { volunteerId = notification.VolunteerId });
        }
    }
}

