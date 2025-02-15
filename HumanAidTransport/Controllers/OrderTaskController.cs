using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HumanitarianTransport.Data;
using HumanAidTransport.Models;

namespace HumanAidTransport.Controllers
{
    public class OrderTaskController : Controller
    {
        private readonly HumanitarianDbContext _context;

        public OrderTaskController(HumanitarianDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> CarrierOrderList(int carrierId)
        {
            // Отримуємо всі DeliveryRequestId для даного CarrierId
            var requestIds = await _context.DeliveryRequests
                .Where(r => r.CarrierId == carrierId)
                .Select(r => r.DeliveryRequestId)
                .ToListAsync();

            if (!requestIds.Any())
            {
                return NotFound(new { message = "Requests not found." });
            }

            // Отримуємо всі замовлення для цих запитів
            var orders = await _context.TransportOrders
                .Where(o => requestIds.Contains(o.DeliveryRequestId))
                .Include(o => o.HumanitarianAid)
                .ToListAsync();

            return View("~/Views/Notification/CarrierTasksList.cshtml", orders);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsCompleted(int orderId)
        {
            // Отримуємо замовлення по ID
            var order = await _context.TransportOrders
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                return NotFound(new { message = "Order not found." });
            }

            order.Status = "Completed";
            await _context.SaveChangesAsync(); // Зберігаємо зміни в базі даних

            // Оновлюємо список всіх замовлень для цього перевізника
            var updatedCarrierOrders = await _context.TransportOrders
                .Where(o => o.HumanAidId == order.HumanAidId)  
                .Include(o => o.HumanitarianAid) 
                .ToListAsync();

            var notification = new Notification
            {
                VolunteerId = order.VolunteerId,
                Message = $"Your assigned task (Order '{order.Name}') has been successfully completed. Thank you for your hard work!",
                CreatedAt = DateTime.UtcNow
            };

            // Повідомлення
            TempData["CompleteMessage"] = "Task completed successfully.";

            // Повертаємо оновлений список замовлень на відповідну вьюху
            return View("~/Views/Notification/CarrierTasksList.cshtml", updatedCarrierOrders);
        }

        [HttpPost]
        public async Task<IActionResult> CancelTask(int orderId)
        {
            // Отримуємо замовлення по ID
            var order = await _context.TransportOrders
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                return NotFound(new { message = "Order not found." });
            }

            // Оновлюємо статус замовлення на "Canceled"
            order.Status = "Canceled";
            await _context.SaveChangesAsync(); // Зберігаємо зміни в базі даних

            TempData["CancelMessage"] = "Task canceled successfully.";

            // Оновлюємо список замовлень для цього перевізника
            var updatedCarrierOrders = await _context.TransportOrders
                .Where(o => o.HumanAidId == order.HumanAidId) // Фільтруємо замовлення за HumanAidId
                .Include(o => o.HumanitarianAid) // Завантажуємо пов'язані дані про гуманітарну допомогу
                .ToListAsync();

            // Повідомлення
            var notification = new Notification
            {
                VolunteerId = order.VolunteerId,
                Message = $"Your assigned task (Order '{order.Name}') has been canceled and is now available for others.",
                CreatedAt = DateTime.UtcNow
            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync(); // Зберігаємо повідомлення

            // Повертаємо оновлений список замовлень
            return View("~/Views/Notification/CarrierTasksList.cshtml", updatedCarrierOrders);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTask(int orderId)
        {
            // Отримуємо замовлення по ID
            var order = await _context.TransportOrders
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            // Якщо замовлення не знайдено, повертаємо помилку
            if (order == null)
            {
                return NotFound(new { message = "Order not found." });
            }

            // Видаляємо замовлення з бази даних
            _context.TransportOrders.Remove(order);
            await _context.SaveChangesAsync(); // Зберігаємо зміни в базі даних

            // Виводимо повідомлення
            TempData["DeleteMessage"] = "Task deleted successfully.";

            // Оновлюємо список замовлень для цього перевізника
            var updatedCarrierOrders = await _context.TransportOrders
                .Where(o => o.HumanAidId == order.HumanAidId) // Фільтруємо замовлення за HumanAidId
                .Include(o => o.HumanitarianAid) // Завантажуємо пов'язані дані про гуманітарну допомогу
                .ToListAsync();

            // Повертаємо оновлений список замовлень до відповідної вьюхи
            return View("~/Views/Notification/CarrierTasksList.cshtml", updatedCarrierOrders);
        }

        public async Task<IActionResult> VolunteerNotifications(int volunteerId)
        {
            // Отримуємо всі сповіщення для даного волонтера
            var notifications = await _context.Notifications
                .Where(n => n.VolunteerId == volunteerId)
                .OrderByDescending(n => n.CreatedAt) 
                .ToListAsync();

            if (!notifications.Any())
            {
                return View("NoNotifications"); //Переробити тут 
            }

            // Повертаємо сповіщення на відповідну вьюху
            return View("~/Views/NotificationVolunteerNotifications", notifications);   //Напиши вьюху для сповіщень і підвяжи кнопку
        }
    }
}