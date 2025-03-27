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

    [HttpPost]
    public async Task<IActionResult> CreateRequest(int carrierId, int humanAidId)
    {
        if (ModelState.IsValid)
        {
            var carrier = await _context.Carriers
                .Include(c => c.Ratings) 
                .FirstOrDefaultAsync(c => c.Id == carrierId);
            var humanitarianAid = await _context.HumanitarianAids.FirstOrDefaultAsync(h => h.HumanAidId == humanAidId);

            if (carrier == null)
                return NotFound(new { message = "Carrier not found." });

            if (humanitarianAid == null)
                return NotFound(new { message = "Humanitarian Aid not found." });

            // Переконуємося, що є волонтер, який відповідає за цю допомогу
            var volunteer = await _context.Volunteers.FirstOrDefaultAsync(v => v.Id == humanitarianAid.VolunteerId);

            if (volunteer == null)
                return NotFound(new { message = "Volunteer not found." });

            var existingRequest = await _context.DeliveryRequests
                .FirstOrDefaultAsync(dr => dr.CarrierId == carrierId && dr.HumanAidId == humanAidId);

            if (existingRequest != null)
            {
                TempData["ErrorMessage"] = "You have already responded to this task.";
                return RedirectToAction("CarrierProfile", "CarrierProfile");
            }

            var deliveryRequest = new DeliveryRequest
            {
                CarrierId = carrierId,
                Carrier = carrier,
                CarrierRating = carrier.AverageRating,
                CarrierContacts = carrier.Contacts,
                VehicleName = carrier.VehicleName,
                VehicleModel = carrier.VehicleModel,
                VehicleNumber = carrier.VehicleNumber,
                HumanAidId = humanAidId,
                HumanitarianAid = humanitarianAid,
                HumanAidName = humanitarianAid.Name,
                VolunteerId = volunteer.Id,
                Volunteer = volunteer,
                Capacity = carrier.Capacity,
                Dimensions = carrier.Dimensions
            };

            humanitarianAid.Status = "Pending";

            _context.DeliveryRequests.Add(deliveryRequest);
            await _context.SaveChangesAsync();

            _context.Carriers.Update(carrier);  
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Thank you for your feedback! Your application has been successfully created.";

            return RedirectToAction("CarrierProfile", "CarrierProfile");
        }

        return BadRequest(new { message = "Invalid data." });
    }

    // Метод для того, щоб волонтер прийняв заявку
    public async Task<IActionResult> AcceptRequest(int deliveryRequestId, int volunteerId)
    {
        // Отримуємо заявку на доставку
        var deliveryRequest = await _context.DeliveryRequests
            .FirstOrDefaultAsync(r => r.DeliveryRequestId == deliveryRequestId);

        if (deliveryRequest == null)
        {
            return NotFound(new { message = "Delivery request not found." });
        }

        // Отримуємо волонтера разом із його заявками і завданнями
        var volunteer = await _context.Volunteers
            .Include(v => v.DeliveryRequests)
            .Include(v => v.Tasks)  // Завантажуємо також завдання волонтера
            .FirstOrDefaultAsync(v => v.Id == volunteerId);

        if (volunteer == null)
        {
            return NotFound(new { message = "Volunteer not found." });
        }

        // Отримуємо гуманітарну допомогу, яка була вказана в заявці
        var humanitarianAid = await _context.HumanitarianAids
            .FirstOrDefaultAsync(h => h.HumanAidId == deliveryRequest.HumanAidId);

        if (humanitarianAid == null)
        {
            return NotFound(new { message = "Humanitarian Aid not found." });
        }

        // Видаляємо заявку з волонтера
        volunteer.DeliveryRequests.Remove(deliveryRequest);

        // Створюємо замовлення після прийняття заявки
        var transportOrder = new TransportOrder
        {
            DeliveryRequestId = deliveryRequest.DeliveryRequestId,
            HumanAidId = humanitarianAid.HumanAidId,
            Name = deliveryRequest.HumanAidName,
            Status = "New",
            ExpectedDeliveryTime = humanitarianAid.ExpectedDeliveryTime,
            Payment = humanitarianAid.Payment,
            DeliveryAddressFrom = humanitarianAid.DeliveryAddressFrom,
            DeliveryAddressTo = humanitarianAid.DeliveryAddressTo,
            VolunteerId = volunteer.Id
        };

        var notification = new Notification
        {
            VolunteerId = volunteer.Id,
            CarrierId = deliveryRequest.CarrierId,
            Message = $"Your task' {deliveryRequest.HumanAidName} ' is comfirmed",
            CreatedAt = DateTime.UtcNow,
            Status = "Comfirmed"

        };
        _context.Notifications.Add(notification);

        // Додаємо нове замовлення до бази даних
        _context.TransportOrders.Add(transportOrder);

        // Оновлюємо статус гуманітарної допомоги
        humanitarianAid.Status = "Confirmed";

        // Зберігаємо зміни в базі даних
        await _context.SaveChangesAsync();

        // Оновлюємо список завдань для волонтера
        var updatedVolunteer = await _context.Volunteers
            .Include(v => v.Tasks)  // Завантажуємо лише завдання волонтера
            .FirstOrDefaultAsync(v => v.Id == volunteerId);

        TempData["AcceptMessage"] = "Delivery request accepted. Transport order created for Carrier.";


        return View("~/Views/Lists/VolunteerRequestList.cshtml", updatedVolunteer.DeliveryRequests);
    }


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

        var humanitarianAid = await _context.HumanitarianAids
           .FirstOrDefaultAsync(h => h.HumanAidId == deliveryRequest.HumanAidId);

        if (humanitarianAid == null)
        {
            return NotFound(new { message = "Humanitarian Aid not found." });
        }

        // Видаляємо заявку у волонтера
        if (volunteer.DeliveryRequests != null)
        {
            volunteer.DeliveryRequests.Remove(deliveryRequest);
        }

        var notification = new Notification
        {
            VolunteerId = volunteer.Id,
            CarrierId = deliveryRequest.CarrierId,
            Message = $"Your task' {deliveryRequest.HumanAidName}' is canceled",
            CreatedAt = DateTime.UtcNow,
            Status = "Canceled"

        };

        _context.Notifications.Add(notification);

        await _context.SaveChangesAsync();

        // Оновлюємо список заявок для волонтера
        var updatedVolunteer = await _context.Volunteers
            .Include(v => v.DeliveryRequests) // Завантажуємо заявки волонтера
            .FirstOrDefaultAsync(v => v.Id == volunteerId);

        TempData["RejectMessage"] = "Delivery request rejected. Removed from volunteer's list.";

        // Повертаємо оновлений список заявок
        return View("~/Views/Lists/VolunteerRequestList.cshtml", updatedVolunteer.DeliveryRequests);
    }
}
