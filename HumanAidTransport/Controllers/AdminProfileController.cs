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

        public IActionResult LogOut()
        {
            Admin = null;
            return RedirectToAction("Index", "Home");
        }
    }
}
