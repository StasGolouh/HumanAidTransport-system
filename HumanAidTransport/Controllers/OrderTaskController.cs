using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HumanitarianTransport.Data;

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

            // Якщо замовлення не знайдено, повертаємо помилку
            if (order == null)
            {
                return NotFound(new { message = "Order not found." });
            }

            // Оновлюємо статус замовлення на "Completed"
            order.Status = "Completed";
            await _context.SaveChangesAsync(); // Зберігаємо зміни в базі даних

            // Виводимо повідомлення
            TempData["CompleteMessage"] = "Task completed successfully.";

            // Оновлюємо список замовлень для цього перевізника
            var updatedCarrierOrders = await _context.TransportOrders
                .Where(o => o.HumanAidId == order.HumanAidId) // Фільтруємо замовлення за HumanAidId
                .Include(o => o.HumanitarianAid) // Завантажуємо пов'язані дані про гуманітарну допомогу
                .ToListAsync();

            // Повертаємо оновлений список замовлень до відповідної вьюхи
            return View("~/Views/Notification/CarrierTasksList.cshtml", updatedCarrierOrders);
        }


        [HttpPost]
        public async Task<IActionResult> CancelTask(int orderId)
        {
            // Отримуємо замовлення по ID
            var order = await _context.TransportOrders
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            // Якщо замовлення не знайдено, повертаємо помилку
            if (order == null)
            {
                return NotFound(new { message = "Order not found." });
            }

            // Оновлюємо статус замовлення на "Canceled"
            order.Status = "Canceled";
            await _context.SaveChangesAsync(); // Зберігаємо зміни в базі даних

            // Виводимо повідомлення
            TempData["CancelMessage"] = "Task canceled successfully.";

            // Оновлюємо список замовлень для цього перевізника
            var updatedCarrierOrders = await _context.TransportOrders
                .Where(o => o.HumanAidId == order.HumanAidId) // Фільтруємо замовлення за HumanAidId
                .Include(o => o.HumanitarianAid) // Завантажуємо пов'язані дані про гуманітарну допомогу
                .ToListAsync();

            // Повертаємо оновлений список замовлень до відповідної вьюхи
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
    }
}