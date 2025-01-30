using HumanAidTransport.Models;
using HumanitarianTransport.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HumanAidTransport.Controllers
{
    public class ProfileController : Controller
    {
        public static Carrier? Carrier;

        private readonly HumanitarianDbContext _context;

        public ProfileController(HumanitarianDbContext context)
        {
            _context = context;
        }

        public IActionResult Profile()
        {
            return View();
        }

        public IActionResult CarrierOrders()
        {
            if (Carrier != null)
            {
                var carrierOrders = _context.CarrierOrders
                    .Where(co => co.CarrierId == Carrier.CarrierId)
                    .Include(co => co.Orders)  
                    .ToList();

                return View(carrierOrders);
            }
            else
            {

                return RedirectToAction("Index", "Home");
            }
        }

        public IActionResult LogOut()
        {
            Carrier = null;  
            return RedirectToAction("Index", "Home"); 
        }
    }
}