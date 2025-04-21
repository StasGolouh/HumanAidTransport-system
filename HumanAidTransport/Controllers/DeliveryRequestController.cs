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
                return NotFound(new { message = "Перевізник не знайдено." });

            if (humanitarianAid == null)
                return NotFound(new { message = "Гуманітарна допомога не знайдена." });

            // Переконуємося, що є волонтер, який відповідає за цю допомогу
            var volunteer = await _context.Volunteers.FirstOrDefaultAsync(v => v.Id == humanitarianAid.VolunteerId);

            if (volunteer == null)
                return NotFound(new { message = "Волонтера не знайдено." });

            var existingRequest = await _context.DeliveryRequests
                .FirstOrDefaultAsync(dr => dr.CarrierId == carrierId && dr.HumanAidId == humanAidId);

            if (existingRequest != null)
            {
                TempData["ErrorMessage"] = "Ви вже відповіли на це завдання.";
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

            humanitarianAid.Status = "В очікуванні";

            _context.DeliveryRequests.Add(deliveryRequest);
            await _context.SaveChangesAsync();

            _context.Carriers.Update(carrier);  
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Дякуємо за відгук! Вашу заявку успішно створено.";

            return RedirectToAction("CarrierProfile", "CarrierProfile");
        }

        return BadRequest(new { message = "Недійсні дані." });
    }

    // Метод для того, щоб волонтер прийняв заявку
    public async Task<IActionResult> AcceptRequest(int deliveryRequestId, int volunteerId)
    {
        // Отримуємо заявку на доставку
        var deliveryRequest = await _context.DeliveryRequests
            .FirstOrDefaultAsync(r => r.DeliveryRequestId == deliveryRequestId);

        if (deliveryRequest == null)
        {
            return NotFound(new { message = "Запит на доставку не знайдено." });
        }

        // Отримуємо волонтера разом із його заявками і завданнями
        var volunteer = await _context.Volunteers
            .Include(v => v.DeliveryRequests)
            .Include(v => v.Tasks)  // Завантажуємо також завдання волонтера
            .FirstOrDefaultAsync(v => v.Id == volunteerId);

        if (volunteer == null)
        {
            return NotFound(new { message = "Волонтера не знайдено." });
        }

        // Отримуємо гуманітарну допомогу, яка була вказана в заявці
        var humanitarianAid = await _context.HumanitarianAids
            .FirstOrDefaultAsync(h => h.HumanAidId == deliveryRequest.HumanAidId);

        if (humanitarianAid == null)
        {
            return NotFound(new { message = "Гуманітарна допомога не знайдена." });
        }

        // Видаляємо заявку з волонтера
        volunteer.DeliveryRequests.Remove(deliveryRequest);

        // Створюємо замовлення після прийняття заявки
        var transportOrder = new TransportOrder
        {
            DeliveryRequestId = deliveryRequest.DeliveryRequestId,
            HumanAidId = humanitarianAid.HumanAidId,
            Name = deliveryRequest.HumanAidName,
            Status = "Новий",
            ExpectedDeliveryTime = humanitarianAid.ExpectedDeliveryTime,
            Payment = humanitarianAid.Payment,
            DeliveryAddressFrom = humanitarianAid.DeliveryAddressFrom,
            DeliveryAddressTo = humanitarianAid.DeliveryAddressTo,
            VolunteerId = volunteer.Id,
            CarrierId = deliveryRequest.CarrierId
        };

        var notification = new Notification
        {
            VolunteerId = volunteer.Id,
            CarrierId = deliveryRequest.CarrierId,
            Message = $"Ваше завдання' {deliveryRequest.HumanAidName} ' підтверженне.",
            CreatedAt = DateTime.UtcNow,
            Status = "Підтверджено"

        };
        _context.Notifications.Add(notification);

        // Додаємо нове замовлення до бази даних
        _context.TransportOrders.Add(transportOrder);

        // Оновлюємо статус гуманітарної допомоги
        humanitarianAid.Status = "Підтверджено";

        // Зберігаємо зміни в базі даних
        await _context.SaveChangesAsync();

        // Оновлюємо список завдань для волонтера
        var updatedVolunteer = await _context.Volunteers
            .Include(v => v.Tasks)  // Завантажуємо лише завдання волонтера
            .FirstOrDefaultAsync(v => v.Id == volunteerId);

        TempData["AcceptMessage"] = "Запит на доставку прийнято. Транспортне замовлення створено для Перевізника.";


        return View("~/Views/Lists/VolunteerRequestList.cshtml", updatedVolunteer.DeliveryRequests);
    }


    [HttpPost]
    public async Task<IActionResult> RejectRequest(int deliveryRequestId, int volunteerId)
    {
        var deliveryRequest = await _context.DeliveryRequests
            .FirstOrDefaultAsync(r => r.DeliveryRequestId == deliveryRequestId);

        if (deliveryRequest == null)
        {
            return NotFound(new { message = "Запит на доставку не знайдено." });
        }

        var volunteer = await _context.Volunteers
            .FirstOrDefaultAsync(v => v.Id == volunteerId);

        if (volunteer == null)
        {
            return NotFound(new { message = "Волонтера не знайдено." });
        }

        var humanitarianAid = await _context.HumanitarianAids
           .FirstOrDefaultAsync(h => h.HumanAidId == deliveryRequest.HumanAidId);

        if (humanitarianAid == null)
        {
            return NotFound(new { message = "Гуманітарна допомога не знайдена." });
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
            Message = $"Ваше завдання' {deliveryRequest.HumanAidName}' скасоване.",
            CreatedAt = DateTime.UtcNow,
            Status = "Скасовано"

        };

        _context.Notifications.Add(notification);

        await _context.SaveChangesAsync();

        // Оновлюємо список заявок для волонтера
        var updatedVolunteer = await _context.Volunteers
            .Include(v => v.DeliveryRequests) // Завантажуємо заявки волонтера
            .FirstOrDefaultAsync(v => v.Id == volunteerId);

        TempData["RejectMessage"] = "Запит на доставку відхилено. Знято зі списку волонтерів.";

        // Повертаємо оновлений список заявок
        return View("~/Views/Lists/VolunteerRequestList.cshtml", updatedVolunteer.DeliveryRequests);
    }
}
