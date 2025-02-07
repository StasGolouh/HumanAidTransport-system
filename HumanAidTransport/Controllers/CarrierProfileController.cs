using HumanitarianTransport.Data;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HumanAidTransport.Controllers
{
    public class CarrierProfileController : Controller
    {
        public static Carrier? Carrier;

        private readonly HumanitarianDbContext _context;

        public CarrierProfileController(HumanitarianDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> CarrierProfile()
        {
            if (Carrier != null)
            {
              
             return View("~/Views/Profile/CarrierProfile.cshtml");
            
            }

            return RedirectToAction("CarrierLogin", "Carrier");
        }

       
        [HttpPost]
        public async Task<IActionResult> UploadPhoto(IFormFile profilePhoto)
        {
            if (profilePhoto == null || profilePhoto.Length == 0)
                return Json(new { success = false, message = "File is not chosen" });

            if (CarrierProfileController.Carrier == null)
                return Unauthorized();

            var carrier = await _context.Carriers.FirstOrDefaultAsync(c => c.Id == CarrierProfileController.Carrier.Id);

            if (carrier == null)
                return NotFound();

            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profile_photos");

            var existingFiles = Directory.GetFiles(uploadsFolder);
            int maxNumber = existingFiles
                .Select(file => Path.GetFileNameWithoutExtension(file))
                .Where(fileName => fileName.StartsWith("photo"))
                .Select(fileName => int.TryParse(fileName.Substring(5), out int num) ? num : 0)
                .DefaultIfEmpty(0)
                .Max();

            string uniqueFileName = "photo" + (maxNumber + 1) + Path.GetExtension(profilePhoto.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await profilePhoto.CopyToAsync(fileStream);
            }

            carrier.ProfilePhotoURL = uniqueFileName;
            _context.Carriers.Update(carrier);
            await _context.SaveChangesAsync();

            return Json(new { success = true, imageUrl = "/images/profile_photos/" + uniqueFileName });
        }

        public IActionResult LogOut()
        {
            Carrier = null;
            return RedirectToAction("Index", "Home");
        }
    }
}
