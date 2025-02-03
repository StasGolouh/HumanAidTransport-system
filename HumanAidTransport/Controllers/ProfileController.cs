﻿using HumanitarianTransport.Data;
using HumanAidTransport.Models;
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
                var carrierOrders = _context.CarrierLists
                    .Where(cl => cl.Carrier != null && cl.Carrier.CarrierId == Carrier.CarrierId)
                    .Include(cl => cl.HumanitarianAid)
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
