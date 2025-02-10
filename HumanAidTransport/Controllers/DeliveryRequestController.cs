using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HumanitarianTransport.Data;
using HumanAidTransport.Models;

public class DeliveryRequestController : Controller
{
    private readonly HumanitarianDbContext _context;

    public DeliveryRequestController(HumanitarianDbContext context)
    {
        _context = context;
    }

    // Створення заявки на доставку
    [HttpPost]
    public async Task<IActionResult> CreateRequest(int carrierId, int humanAidId)
    {
        if (ModelState.IsValid)
        {
            var carrier = await _context.Carriers.FirstOrDefaultAsync(c => c.Id == carrierId);
            var humanitarianAid = await _context.HumanitarianAids.FirstOrDefaultAsync(h => h.HumanAidId == humanAidId);

            if (carrier == null)
                return NotFound(new { message = "Carrier not found." });

            if (humanitarianAid == null)
                return NotFound(new { message = "Humanitarian Aid not found." });

            // Створюємо нову заявку на доставку
            var deliveryRequest = new DeliveryRequest
            {
                CarrierId = carrierId,
                HumanAidId = humanAidId,
                CarrierRating = carrier.Rating,
                CarrierContacts = carrier.Contacts,
                VehicleName = carrier.VehicleName, 
                VehicleModel = carrier.VehicleModel,
                VehicleNumber = carrier.VehicleNumber,
                HumanAidName = humanitarianAid.Name
            };

            _context.DeliveryRequests.Add(deliveryRequest);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Delivery request created successfully!",
                requestId = deliveryRequest.DeliveryRequestId
            });
        }

        return BadRequest(new { message = "Invalid data." });
    }

    // Метод для того, щоб волонтер прийняв заявку
    [HttpPost]
    public async Task<IActionResult> AcceptRequest(int deliveryRequestId, int volunteerId)
    {
        // Отримуємо заявку на доставку
        var deliveryRequest = await _context.DeliveryRequests
             .FirstOrDefaultAsync(r => r.DeliveryRequestId == deliveryRequestId);

        if (deliveryRequest == null)
        {
            return NotFound(new { message = "Delivery request not found." });
        }

        // Отримуємо волонтера
        var volunteer = await _context.Volunteers
            .FirstOrDefaultAsync(v => v.Id == volunteerId);

        if (volunteer == null)
        {
            return NotFound(new { message = "Volunteer not found." });
        }

        // Прив'язуємо волонтера до заявки
        deliveryRequest.VolunteerId = volunteer.Id;

        // Отримуємо гуманітарну допомогу, яка була вказана в заявці
        var humanitarianAid = await _context.HumanitarianAids
            .FirstOrDefaultAsync(h => h.HumanAidId == deliveryRequest.HumanAidId);

        if (humanitarianAid == null)
        {
            return NotFound(new { message = "Humanitarian Aid not found." });
        }

        // Створюємо замовлення після прийняття заявки
        var transportOrder = new TransportOrder
        {
            DeliveryRequestId = deliveryRequest.DeliveryRequestId,
            HumanAidId = humanitarianAid.HumanAidId,
            Name = deliveryRequest.HumanAidName,
            CreatedAt = DateTime.UtcNow,
            ExpectedDeliveryTime = humanitarianAid.ExpectedDeliveryTime,
            Payment = humanitarianAid.Payment,
            DeliveryAddressFrom = humanitarianAid.DeliveryAddressFrom,
            DeliveryAddressTo = humanitarianAid.DeliveryAddressTo
        };

        // Додаємо нове замовлення до бази даних
        _context.TransportOrders.Add(transportOrder);

        // Зберігаємо зміни в базі даних
        await _context.SaveChangesAsync();

        return Ok(new { message = "Delivery request accepted by volunteer and transport order created." });
    }

    // Метод для того, щоб волонтер відхилив заявку
    [HttpPost]
    public async Task<IActionResult> RejectRequest(int deliveryRequestId, int volunteerId)
    {
        var deliveryRequest = await _context.DeliveryRequests
            .FirstOrDefaultAsync(r => r.DeliveryRequestId == deliveryRequestId);

        if (deliveryRequest == null)
        {
            return NotFound(new { message = "Delivery request not found." });
        }

        var volunteer = await _context.Volunteers
            .FirstOrDefaultAsync(v => v.Id == volunteerId);

        if (volunteer == null)
        {
            return NotFound(new { message = "Volunteer not found." });
        }

        // Відхиляємо заявку (просто залишаємо її без волонтера)
        deliveryRequest.VolunteerId = null;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Delivery request rejected by volunteer." });
    }

    // Метод для отримання списку заявок для волонтера
    public async Task<IActionResult> VolunteerRequestList(int volunteerId)
    {
        var volunteer = await _context.Volunteers
            .FirstOrDefaultAsync(v => v.Id == volunteerId);

        if (volunteer == null)
        {
            return NotFound(new { message = "Volunteer not found." });
        }

        var requests = await _context.DeliveryRequests
            .Where(r => r.VolunteerId == null) // Тільки не прийняті заявки
            .ToListAsync();

        return View(requests);
    }
}
