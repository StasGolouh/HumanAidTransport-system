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

            // Отримуємо всі замовлення для цих запитів
            var orders = await _context.TransportOrders
                .Where(o => requestIds.Contains(o.DeliveryRequestId))
                .Include(o => o.HumanitarianAid)
                .ToListAsync();

            return View("~/Views/Notification/CarrierTasksList.cshtml", orders);
        }


        [HttpPost]
        public async Task<IActionResult> MarkAsInProgress(int orderId)
        {
            var order = await _context.TransportOrders.FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                return NotFound(new { message = "Order not found." });
            }

            var deliveryRequest = _context.DeliveryRequests.FirstOrDefault(dr => dr.DeliveryRequestId == order.DeliveryRequestId);

            order.Status = "In progress";
            await _context.SaveChangesAsync();

            var humanAid = _context.HumanitarianAids.FirstOrDefault(humanAid => humanAid.HumanAidId == deliveryRequest.HumanAidId);

            humanAid.Status = "In progress";

            var notification = new Notification
            {
                VolunteerId = order.VolunteerId,
                CarrierId = deliveryRequest.CarrierId,
                Message = $"Your assigned task (order '{order.Name}') is in progress",
                CreatedAt = DateTime.UtcNow,
                Status = "In progress"

            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            TempData["ProgressMessage"] = "Task was marked in progress.";

            var updatedCarrierOrders = await _context.TransportOrders
                .Where(o => o.HumanAidId == order.HumanAidId)
                .Include(o => o.HumanitarianAid)
                .ToListAsync();

            return RedirectToAction("CarrierOrderList", new { carrierId = deliveryRequest.CarrierId });
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsCompleted(int orderId)
        {
            var order = await _context.TransportOrders.FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                return NotFound(new { message = "Order not found." });
            }
            
            var deliveryRequest = _context.DeliveryRequests.FirstOrDefault(dr => dr.DeliveryRequestId == order.DeliveryRequestId);

            order.Status = "Completed";
            await _context.SaveChangesAsync();

            var humanAid = _context.HumanitarianAids.FirstOrDefault(humanAid => humanAid.HumanAidId == deliveryRequest.HumanAidId);

            humanAid.Status = "Completed";

            var notification = new Notification
            {
                VolunteerId = order.VolunteerId,
                CarrierId = deliveryRequest.CarrierId,
                Message = $"Your assigned task (Order '{order.Name}') has been successfully completed",
                CreatedAt = DateTime.UtcNow,
                Status = "Completed"

            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            TempData["CompleteMessage"] = "Task completed successfully.";

            var updatedCarrierOrders = await _context.TransportOrders
                .Where(o => o.HumanAidId == order.HumanAidId)
                .Include(o => o.HumanitarianAid)
                .ToListAsync();

            return RedirectToAction("CarrierOrderList", new { carrierId = deliveryRequest.CarrierId });
        }

        [HttpPost]
        public async Task<IActionResult> CancelTask(int orderId)
        {
            var order = await _context.TransportOrders.FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order == null)
            {
                return NotFound(new { message = "Order not found." });
            }

            var deliveryRequest = _context.DeliveryRequests.FirstOrDefault(dr => dr.DeliveryRequestId == order.DeliveryRequestId);

            order.Status = "Canceled";
            await _context.SaveChangesAsync();

            var humanAid = _context.HumanitarianAids.FirstOrDefault(humanAid => humanAid.HumanAidId == deliveryRequest.HumanAidId);

            humanAid.Status = "Rejected";

            var notification = new Notification
            {
                VolunteerId = order.VolunteerId,
                CarrierId = deliveryRequest.CarrierId,
                Message = $"Your assigned task (Order '{order.Name}') has been canceled and is now available for others.",
                CreatedAt = DateTime.UtcNow,
                Status = "Canceled"
            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            TempData["CancelMessage"] = "Task canceled successfully.";

            var updatedCarrierOrders = await _context.TransportOrders
                .Where(o => o.HumanAidId == order.HumanAidId)
                .Include(o => o.HumanitarianAid)
                .ToListAsync();

            return RedirectToAction("CarrierOrderList", new { carrierId = deliveryRequest.CarrierId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTask(int orderId)
        {
            var order = await _context.TransportOrders
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            var deliveryRequest = _context.DeliveryRequests.FirstOrDefault(dr => dr.DeliveryRequestId == order.DeliveryRequestId);

            _context.TransportOrders.Remove(order);
            await _context.SaveChangesAsync(); 

            TempData["DeleteMessage"] = "Task deleted successfully.";

            var updatedCarrierOrders = await _context.TransportOrders
                .Where(o => o.HumanAidId == order.HumanAidId) 
                .Include(o => o.HumanitarianAid) 
                .ToListAsync();

            return RedirectToAction("CarrierOrderList", new { carrierId = deliveryRequest.CarrierId });
        }
    }
}