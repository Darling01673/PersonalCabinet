using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalCabinet.Models;
using PersonalCabinet.ViewModels;
using System.Security.Claims;

namespace PersonalCabinet.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string consent = Request.Form["PersonalDataConsent"];
            if (consent != "true" && consent != "on" && consent != "True")
            {
                ModelState.AddModelError("", "Необходимо согласие на обработку персональных данных");
                return View(model);
            }

            bool emailExists = await _context.Users.AnyAsync(u => u.Email == model.Email);
            if (emailExists)
            {
                ModelState.AddModelError("Email", "Пользователь с таким email уже существует");
                return View(model);
            }

            if (model.UserType == "Organization")
            {
                if (string.IsNullOrWhiteSpace(model.OrgFullName))
                {
                    ModelState.AddModelError("OrgFullName", "Полное наименование обязательно");
                }
                if (string.IsNullOrWhiteSpace(model.OrgShortName))
                {
                    ModelState.AddModelError("OrgShortName", "Сокращённое наименование обязательно");
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(model.FullName))
                {
                    ModelState.AddModelError("FullName", "ФИО обязательно");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            User user = new User();
            user.Email = model.Email;
            user.Phone = model.Phone;
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
            user.Role = "USER";
            user.UserType = MapUserType(model.UserType);
            user.CreatedAt = DateTime.Now;
            user.UpdatedAt = DateTime.Now;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            if (model.UserType == "Organization")
            {
                OrganizationProfile orgProfile = new OrganizationProfile();
                orgProfile.UserId = user.Id;
                orgProfile.FullName = model.OrgFullName;
                orgProfile.ShortName = model.OrgShortName;
                orgProfile.ContactPerson = model.ContactPerson;
                _context.OrganizationProfiles.Add(orgProfile);
            }
            else
            {
                IndividualProfile indProfile = new IndividualProfile();
                indProfile.UserId = user.Id;

                string[] parts = model.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 0) indProfile.LastName = parts[0];
                if (parts.Length > 1) indProfile.FirstName = parts[1];
                if (parts.Length > 2) indProfile.MiddleName = parts[2];

                _context.IndividualProfiles.Add(indProfile);
            }
            await _context.SaveChangesAsync();

            await SignInAsync(user);

            if (user.Role == "Admin")
                return RedirectToAction("MainMenuAdmin", "Admin");
            else
                return RedirectToAction("MainMenu", "Home");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            User user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Неверный email или пароль");
                return View(model);
            }

            bool passwordOk = BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash);
            if (!passwordOk)
            {
                ModelState.AddModelError("", "Неверный email или пароль");
                return View(model);
            }

            await SignInAsync(user, model.RememberMe);

            if (user.Role == "Admin")
                return RedirectToAction("MainMenuAdmin", "Admin");
            else
                return RedirectToAction("MainMenu", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            User user = await _context.Users
                .Include(u => u.IndividualProfile)
                .Include(u => u.OrganizationProfile)
                .Include(u => u.PersonalData)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            ProfileViewModel model = new ProfileViewModel();
            model.Id = user.Id;
            model.Email = user.Email;
            model.Phone = user.Phone;
            model.UserType = user.UserType;

            if (user.UserType == "INDIVIDUAL" && user.IndividualProfile != null)
            {
                model.LastName = user.IndividualProfile.LastName;
                model.FirstName = user.IndividualProfile.FirstName;
                model.MiddleName = user.IndividualProfile.MiddleName;
            }
            else if (user.UserType == "ORGANIZATION" && user.OrganizationProfile != null)
            {
                model.OrgFullName = user.OrganizationProfile.FullName;
                model.OrgShortName = user.OrganizationProfile.ShortName;
                model.ContactPerson = user.OrganizationProfile.ContactPerson;
            }

            if (user.PersonalData != null)
            {
                model.ResidenceAddress = user.PersonalData.ResidenceAddress;
                model.Inn = user.PersonalData.Inn;
                model.PassportSeries = user.PersonalData.PassportSeries;
                model.PassportNumber = user.PersonalData.PassportNumber;
                model.PassportDate = user.PersonalData.PassportDate;
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (model.UserType == "INDIVIDUAL")
            {
                ModelState.Remove("OrgFullName");
                ModelState.Remove("OrgShortName");
                ModelState.Remove("ContactPerson");
                ModelState.Remove("ResidenceAddress");
                ModelState.Remove("Inn");
                ModelState.Remove("PassportSeries");
                ModelState.Remove("PassportNumber");
                ModelState.Remove("PassportDate");
            }

            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                TempData["ErrorMessage"] = "Ошибка валидации: " + errors;
                return View(model);
            }

            try
            {
                long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var user = await _context.Users
                    .Include(u => u.IndividualProfile)
                    .Include(u => u.OrganizationProfile)
                    .Include(u => u.PersonalData)
                    .FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null) return NotFound();

                user.Email = model.Email;
                user.Phone = model.Phone;
                user.UpdatedAt = DateTime.Now;

                if (user.UserType == "INDIVIDUAL")
                {
                    if (user.IndividualProfile == null)
                        user.IndividualProfile = new IndividualProfile { UserId = user.Id };
                    user.IndividualProfile.LastName = model.LastName;
                    user.IndividualProfile.FirstName = model.FirstName;
                    user.IndividualProfile.MiddleName = model.MiddleName;
                }
                else if (user.UserType == "ORGANIZATION")
                {
                    if (user.OrganizationProfile == null)
                        user.OrganizationProfile = new OrganizationProfile { UserId = user.Id };
                    user.OrganizationProfile.FullName = model.OrgFullName;
                    user.OrganizationProfile.ShortName = model.OrgShortName;
                    user.OrganizationProfile.ContactPerson = model.ContactPerson;
                }

                bool hasPersonal = !string.IsNullOrWhiteSpace(model.ResidenceAddress) ||
                                   !string.IsNullOrWhiteSpace(model.Inn) ||
                                   !string.IsNullOrWhiteSpace(model.PassportSeries) ||
                                   !string.IsNullOrWhiteSpace(model.PassportNumber) ||
                                   model.PassportDate.HasValue;
                if (hasPersonal)
                {
                    if (user.PersonalData == null)
                    {
                        user.PersonalData = new UserPersonalData { UserId = user.Id };
                        _context.UserPersonalData.Add(user.PersonalData);
                    }
                    user.PersonalData.ResidenceAddress = model.ResidenceAddress;
                    user.PersonalData.Inn = model.Inn;
                    user.PersonalData.PassportSeries = model.PassportSeries;
                    user.PersonalData.PassportNumber = model.PassportNumber;
                    user.PersonalData.PassportDate = model.PassportDate;
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Профиль успешно обновлён";
            }
            catch (DbUpdateException ex)
            {
                TempData["ErrorMessage"] = "Ошибка БД: " + (ex.InnerException?.Message ?? ex.Message);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Ошибка: " + ex.Message;
            }
            return RedirectToAction("Profile");
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            User user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            bool currentOk = BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.PasswordHash);
            if (!currentOk)
            {
                ModelState.AddModelError("", "Текущий пароль введён неверно");
                return View(model);
            }

            if (model.NewPassword.Length < 6)
            {
                ModelState.AddModelError("NewPassword", "Пароль должен быть не менее 6 символов");
                return View(model);
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            user.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Пароль успешно изменён";
            return RedirectToAction("Profile");
        }

        [HttpGet]
        public async Task<IActionResult> GetPersonalData()
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            User user = await _context.Users
                .Include(u => u.IndividualProfile)
                .Include(u => u.PersonalData)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            var result = new
            {
                LastName = user.IndividualProfile?.LastName,
                FirstName = user.IndividualProfile?.FirstName,
                MiddleName = user.IndividualProfile?.MiddleName,
                Phone = user.Phone,
                ResidenceAddress = user.PersonalData?.ResidenceAddress,
                Inn = user.PersonalData?.Inn,
                PassportSeries = user.PersonalData?.PassportSeries,
                PassportNumber = user.PersonalData?.PassportNumber,
                PassportDate = user.PersonalData?.PassportDate?.ToString("yyyy-MM-dd")
            };
            return Ok(result);
        }

        private async Task SignInAsync(User user, bool isPersistent = false)
        {
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Email),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Role),
        new Claim("UserType", user.UserType)
    };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = isPersistent,
                ExpiresUtc = isPersistent ? DateTimeOffset.UtcNow.AddDays(1) : (DateTimeOffset?)null
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);
        }

        private string MapUserType(string formType)
        {
            if (formType == "Individual") return "INDIVIDUAL";
            if (formType == "IndividualEntrepreneur") return "ENTREPRENEUR";
            if (formType == "Organization") return "ORGANIZATION";
            if (formType == "Representative") return "REPRESENTATIVE";
            return "INDIVIDUAL";
        }
    }
}