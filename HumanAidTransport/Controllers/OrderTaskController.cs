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
   
            var orders = await _context.TransportOrders
                .Where(o => o.CarrierId == carrierId)
                .Include(o => o.HumanitarianAid)
                .ToListAsync();

            return View("~/Views/Lists/CarrierTasksList.cshtml", orders);
        }


        [HttpPost]
        public async Task<IActionResult> MarkAsInProgress(int orderId)
        {
            var order = await _context.TransportOrders.FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                return NotFound(new { message = "Замовлення не знайдено." });
            }

            var deliveryRequest = _context.DeliveryRequests.FirstOrDefault(dr => dr.DeliveryRequestId == order.DeliveryRequestId);

            order.Status = "В процесі";
            await _context.SaveChangesAsync();

            var humanAid = _context.HumanitarianAids.FirstOrDefault(humanAid => humanAid.HumanAidId == deliveryRequest.HumanAidId);

            humanAid.Status = "В процесі";

            var notification = new Notification
            {
                VolunteerId = order.VolunteerId,
                CarrierId = order.CarrierId,
                Message = $"Ваше призначене завдання (Замовлення '{order.Name}') у процесі",
                CreatedAt = DateTime.UtcNow,
                Status = "В процесі"

            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            TempData["ProgressMessage"] = "Завдання позначено як у процесі.";

            var updatedCarrierOrders = await _context.TransportOrders
                .Where(o => o.HumanAidId == order.HumanAidId)
                .Include(o => o.HumanitarianAid)
                .ToListAsync();

            return RedirectToAction("CarrierOrderList", new { carrierId = order.CarrierId });
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsCompleted(int orderId, int enteredCode)
        {
            var order = await _context.TransportOrders.FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                return NotFound(new { message = "Замовлення не знайдено." });
            }

            var deliveryRequest = await _context.DeliveryRequests.FirstOrDefaultAsync(dr => dr.DeliveryRequestId == order.DeliveryRequestId);
            var humanAid = await _context.HumanitarianAids.FirstOrDefaultAsync(h => h.HumanAidId == deliveryRequest.HumanAidId);

            // Перевірка PIN-коду має бути першою
            if (humanAid == null || enteredCode != humanAid.PinCode)
            {
                TempData["CancelMessage"] = "❌ Невірний PIN-код. Спробуйте ще раз.";
                return RedirectToAction("CarrierOrderList", new { carrierId = order.CarrierId });
            }

            // PIN правильний, тепер оновлюємо статуси
            order.Status = "Виконано";
            humanAid.Status = "Виконано";

            humanAid.CompletedAt = DateTime.Now;

            var notification = new Notification
            {
                VolunteerId = order.VolunteerId,
                CarrierId = order.CarrierId,
                Message = $"Ваше призначене завдання (Замовлення '{order.Name}') було успішно виконано. Сплатіть доставку в найближчі 3 години, " +
                $"аби не отримати 1,5 кратний штраф",
                CreatedAt = DateTime.UtcNow,
                Status = "Виконано"
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            TempData["CompleteMessage"] = "✅ Завдання виконано успішно.";

            return RedirectToAction("CarrierOrderList", new { carrierId = order.CarrierId });
        }

        [HttpPost]
        public async Task<IActionResult> CancelTask(int orderId)
        {
            var order = await _context.TransportOrders.FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order == null)
            {
                return NotFound(new { message = "Замовлення не знайдено." });
            }

            var deliveryRequest = _context.DeliveryRequests.FirstOrDefault(dr => dr.DeliveryRequestId == order.DeliveryRequestId);

            order.Status = "Відхилено";
            await _context.SaveChangesAsync();

            var humanAid = _context.HumanitarianAids.FirstOrDefault(humanAid => humanAid.HumanAidId == deliveryRequest.HumanAidId);

            humanAid.Status = "Відхилено";

            var notification = new Notification
            {
                VolunteerId = order.VolunteerId,
                CarrierId = order.CarrierId,
                Message = $"Ваше призначене завдання (Замовлення '{order.Name}') було скасовано, і ви можете зробити його доступним для інших.",
                CreatedAt = DateTime.UtcNow,
                Status = "Відхилено"
            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            TempData["CancelMessage"] = "Завдання успішно відхилено.";

            var updatedCarrierOrders = await _context.TransportOrders
                .Where(o => o.HumanAidId == order.HumanAidId)
                .Include(o => o.HumanitarianAid)
                .ToListAsync();

            return RedirectToAction("CarrierOrderList", new { carrierId = order.CarrierId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTask(int orderId)
        {
            var order = await _context.TransportOrders
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            _context.TransportOrders.Remove(order);
            await _context.SaveChangesAsync(); 

            TempData["DeleteMessage"] = "Завдання успішно видалено.";

            var updatedCarrierOrders = await _context.TransportOrders
                .Where(o => o.HumanAidId == order.HumanAidId) 
                .Include(o => o.HumanitarianAid) 
                .ToListAsync();

            return RedirectToAction("CarrierOrderList", new { carrierId = order.CarrierId });
        }
    }
}