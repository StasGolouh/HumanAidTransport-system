using HumanitarianTransport.Data;
using Microsoft.AspNetCore.Mvc;


namespace HumanAidTransport.Controllers
{
    public class VolunteerController : Controller
    {
        private readonly HumanitarianDbContext _context;

        public VolunteerController(HumanitarianDbContext context)
        {
            _context = context;
        }

        public IActionResult VolunteerRegistration()
        {
            return View("~/Views/Registration/VolunteerRegistration.cshtml");
        }

        [HttpPost]
        public IActionResult VolunteerRegistration(Volunteer volunteer)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(volunteer.Name ?? "", @"^[a-zA-Zа-яА-ЯіІїЇєЄґҐ0-9\s]{2,}$"))
            {
                ModelState.AddModelError("Name", "Ім'я може містити лише літери, цифри та пробіли (мінімум 2 символи).");
            }

            if (string.IsNullOrWhiteSpace(volunteer.Password) || volunteer.Password.Length < 8 || volunteer.Password.Contains(" "))
            {
                ModelState.AddModelError("Password", "Пароль має бути не менше 8 символів і не містити пробілів.");
            }

            // Додаткова перевірка на ім’я
            bool nameUsedByCarrier = _context.Carriers.Any(c => c.Name == volunteer.Name);
            if (nameUsedByCarrier)
            {
                ModelState.AddModelError("Name", "Це ім’я вже використовується перевізником. Оберіть інше.");
            }

            // Перевірка платіжних даних
            if (!System.Text.RegularExpressions.Regex.IsMatch(volunteer.CardNumber ?? "", @"^\d{16}$"))
                ModelState.AddModelError("CardNumber", "Номер картки має складатися з 16 цифр.");

            if (!System.Text.RegularExpressions.Regex.IsMatch(volunteer.CVV ?? "", @"^\d{3}$"))
                ModelState.AddModelError("CVV", "CVV має складатися з 3 цифр.");


            bool cardUsed = _context.Volunteers.Any(v => v.CardNumber == volunteer.CardNumber) ||
                _context.Carriers.Any(c => c.CardNumber == volunteer.CardNumber);

            if (cardUsed)
            {
                ModelState.AddModelError("CardNumber", "Цей номер картки вже використовується.");
            }

            if (ModelState.IsValid)
            {
                bool volunteerExists = _context.Volunteers.Any(v => v.Name == volunteer.Name);

                if (!volunteerExists)
                {
                    try
                    {
                        _context.Volunteers.Add(volunteer);
                        _context.SaveChanges();

                        TempData["RegistMessage"] = "Реєстрація пройшла успішно, будь ласка, увійдіть.";
                        return RedirectToAction("VolunteerProfile", "VolunProfile");
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Помилка збереження волонтера: " + ex.Message);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Волонтер з таким іменем вже існує.");
                }
            }

            return View("~/Views/Registration/VolunteerRegistration.cshtml");
        }



        //=============================Login====================================

        public IActionResult VolunteerLogin()
        {
            return View("~/Views/Login/VolunteerLogin.cshtml");
        }

        [HttpPost]
        public IActionResult VolunteerLogin(string name, string password)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError(string.Empty, "Будь ласка, заповніть усі поля.");
                return View("~/Views/Login/VolunteerLogin.cshtml");
            }

            // Валідація імені
            if (!System.Text.RegularExpressions.Regex.IsMatch(name ?? "", @"^[a-zA-Zа-яА-ЯіІїЇєЄґҐ0-9\s]{2,}$"))
            {
                ModelState.AddModelError("name", "Ім'я може містити лише літери, цифри та пробіли (мінімум 2 символи).");
                return View("~/Views/Login/VolunteerLogin.cshtml");
            }

            // Валідація пароля
            if (password.Length < 8 || password.Contains(" "))
            {
                ModelState.AddModelError("password", "Пароль має бути не менше 8 символів і не містити пробілів.");
                return View("~/Views/Login/VolunteerLogin.cshtml");
            }

            var volunteer = _context.Volunteers.FirstOrDefault(c => c.Name == name && c.Password == password);

            if (volunteer != null)
            {
                VolunProfileController.Volunteer = volunteer;
                HttpContext.Session.SetString("UserName", name);
                return RedirectToAction("VolunteerProfile", "VolunProfile");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Неправильний логін або пароль для волонтера.");
                return View("~/Views/Login/VolunteerLogin.cshtml");
            }
        }
    }
}
