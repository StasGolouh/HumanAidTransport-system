using HumanAidTransport.Models;
using HumanitarianTransport.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HumanAidTransport.Controllers
{
    public class AdminProfileController : Controller
    {
        public static Admin? Admin;

        private readonly HumanitarianDbContext _context;

        public AdminProfileController(HumanitarianDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> AdminProfile()
        {
            if (Admin != null)
            {
                var admin = await _context.Admins
                    .FirstOrDefaultAsync(ad => ad.Id == Admin.Id);

                if (admin != null)
                {
                    admin.Carriers = await _context.Carriers.ToListAsync();
                    admin.Volunteers = await _context.Volunteers.ToListAsync();

                    return View("~/Views/Profile/AdminProfile.cshtml", admin);
                }
            }

            return RedirectToAction("AdminLogin", "Admin");
        }

        [HttpPost]
        public async Task<IActionResult> ChangeCarrierBanStatus(int id, bool isBanned)
        {
            var carrierFromDb = await _context.Carriers.FirstOrDefaultAsync(c => c.Id == id);

            if (carrierFromDb == null)
            {
                TempData["ErrorMessage"] = "Перевізник не знайдений.";
                return RedirectToAction("AdminProfile");
            }

            if (carrierFromDb.isBaned == isBanned)
            {
                TempData["ErrorMessage"] = isBanned
                    ? $"Перевізник {carrierFromDb.Name} вже заблокований."
                    : $"Перевізник {carrierFromDb.Name} вже розблокований.";
                return RedirectToAction("AdminProfile");
            }

            if (isBanned && carrierFromDb.ViolationsCount < 2)
            {
                TempData["ErrorMessage"] = $"Перевізника {carrierFromDb.Name} не можна заблокувати, оскільки він має менше 2 порушень.";
                return RedirectToAction("AdminProfile");
            }

            carrierFromDb.isBaned = isBanned;

            _context.Notifications.Add(new Notification
            {
                CarrierId = carrierFromDb.Id,
                VolunteerId = null,
                Message = isBanned
                    ? $"Ваш акаунт перевізника заблоковано адміністратором через 2 або більше порушень. Ви маєте можливість довиконати ті завдання, " +
                    $"які ще не закінчили, поповнити баланс та змінити фото."
                    : $"Вітання. Ваш акаунт перевізника розблоковано адміністратором.",
                CreatedAt = DateTime.UtcNow,
                Status = "Блокування Перевізника"
            });

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = isBanned
                ? $"Перевізник {carrierFromDb.Name} заблокований."
                : $"Перевізник {carrierFromDb.Name} розблокований.";

            return RedirectToAction("AdminProfile");
        }


        [HttpPost]
        public async Task<IActionResult> ChangeVolunteerBanStatus(int id, bool isBanned)
        {
            var volunteerFromDb = await _context.Volunteers.FirstOrDefaultAsync(v => v.Id == id);

            if (volunteerFromDb == null)
            {
                TempData["ErrorMessage"] = "Волонтер не знайдений.";
                return RedirectToAction("AdminProfile");
            }

            if (volunteerFromDb.isBaned == isBanned)
            {
                TempData["ErrorMessage"] = isBanned
                    ? $"Волонтер {volunteerFromDb.Name} вже заблокований."
                    : $"Волонтер {volunteerFromDb.Name} вже розблокований.";
                return RedirectToAction("AdminProfile");
            }

            if (isBanned && volunteerFromDb.ViolationsCount < 2)
            {
                TempData["ErrorMessage"] = $"Волонтера {volunteerFromDb.Name} не можна заблокувати, оскільки він має менше 2 порушень.";
                return RedirectToAction("AdminProfile");
            }

            volunteerFromDb.isBaned = isBanned;

            _context.Notifications.Add(new Notification
            {
                VolunteerId = volunteerFromDb.Id,
                CarrierId = null,
                Message = isBanned
                    ? $"Ваш акаунт волонтера заблоковано адміністратором через 2 або більше порушень. Ви маєте можливість лише на поповнення балансу, оплату закінчених завдань."
                    : $"Вітання. Ваш акаунт волонтера розблоковано адміністратором.",
                CreatedAt = DateTime.UtcNow,
                Status = "Блокування Волонтера"
            });

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = isBanned
                ? $"Волонтер {volunteerFromDb.Name} заблокований."
                : $"Волонтер {volunteerFromDb.Name} розблокований.";

            return RedirectToAction("AdminProfile");
        }

        public IActionResult LogOut()
        {
            Admin = null;
            return RedirectToAction("Index", "Home");
        }
    }
}
