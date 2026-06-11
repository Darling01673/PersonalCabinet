using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PersonalCabinet.Models;
using PersonalCabinet.ViewModels;
using System.Security.Claims;

namespace PersonalCabinet.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;

        public AccountController(ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
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
            var formUserType = Request.Form["UserType"].ToString();
            if (!string.IsNullOrEmpty(formUserType))
                model.UserType = formUserType;
            string consent = Request.Form["PersonalDataConsent"];
            if (consent != "true" && consent != "on" && consent != "True")
                ModelState.AddModelError("", "Необходимо согласие на обработку персональных данных");
            bool emailExists = await _context.Users.AnyAsync(u => u.Email == model.Email);
            if (emailExists)
                ModelState.AddModelError("Email", "Пользователь с таким email уже существует");
            string confirmPassword = Request.Form["ConfirmPassword"];
            if (model.Password != confirmPassword)
                ModelState.AddModelError("ConfirmPassword", "Пароли не совпадают");
            if (model.UserType == "Organization")
            {
                ModelState.Remove("FullName");
                if (string.IsNullOrWhiteSpace(model.OrgFullName))
                    ModelState.AddModelError("OrgFullName", "Полное наименование обязательно");
                if (string.IsNullOrWhiteSpace(model.OrgShortName))
                    ModelState.AddModelError("OrgShortName", "Сокращённое наименование обязательно");
                if (string.IsNullOrWhiteSpace(model.ContactPerson))
                    ModelState.AddModelError("ContactPerson", "Контактное лицо обязательно");
            }
            else
            {
                ModelState.Remove("OrgFullName");
                ModelState.Remove("OrgShortName");
                ModelState.Remove("ContactPerson");
                if (string.IsNullOrWhiteSpace(model.FullName))
                    ModelState.AddModelError("FullName", "ФИО обязательно");
            }
            if (!ModelState.IsValid)
                return View(model);
            User user = new User
            {
                Email = model.Email,
                Phone = model.Phone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Role = "USER",
                UserType = MapUserType(model.UserType),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            if (model.UserType == "Organization")
            {
                _context.OrganizationProfiles.Add(new OrganizationProfile
                {
                    UserId = user.Id,
                    FullName = model.OrgFullName,
                    ShortName = model.OrgShortName,
                    ContactPerson = model.ContactPerson
                });
            }
            else
            {
                var parts = model.FullName?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
                _context.IndividualProfiles.Add(new IndividualProfile
                {
                    UserId = user.Id,
                    LastName = parts.Length > 0 ? parts[0] : "",
                    FirstName = parts.Length > 1 ? parts[1] : "",
                    MiddleName = parts.Length > 2 ? parts[2] : ""
                });
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
            if (TempData.ContainsKey("ErrorMessage"))
                TempData.Remove("ErrorMessage");

            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string cacheKey = $"login_attempts_{model.Email}";
            var attemptData = _cache.Get<(int Attempts, DateTime? LockoutEnd)>(cacheKey);
            int failedAttempts = attemptData.Attempts;
            DateTime? lockoutEnd = attemptData.LockoutEnd;
            if (lockoutEnd.HasValue && lockoutEnd > DateTime.UtcNow)
            {
                int remainingMinutes = (int)(lockoutEnd.Value - DateTime.UtcNow).TotalMinutes;
                if (remainingMinutes < 1) remainingMinutes = 1;
                ModelState.AddModelError("", $"Слишком много неудачных попыток. Попробуйте через {remainingMinutes} минут.");
                return View(model);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            bool isPasswordValid = user != null && BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash);

            if (!isPasswordValid)
            {
                failedAttempts++;
                lockoutEnd = null;

                if (failedAttempts >= 3)
                {
                    lockoutEnd = DateTime.UtcNow.AddMinutes(10);
                    ModelState.AddModelError("", "Превышено количество попыток входа. Аккаунт заблокирован на 10 минут.");
                }
                else
                {
                    int remainingAttempts = 3 - failedAttempts;
                    ModelState.AddModelError("", $"Неверный email или пароль. Осталось попыток: {remainingAttempts}.");
                }

                _cache.Set(cacheKey, (failedAttempts, lockoutEnd), TimeSpan.FromMinutes(15));
                return View(model);
            }
            _cache.Remove(cacheKey);
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
            var user = await _context.Users
                .Include(u => u.IndividualProfile)
                .Include(u => u.OrganizationProfile)
                .Include(u => u.PersonalData)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound();

            var model = new ProfileViewModel
            {
                Id = user.Id,
                Email = user.Email,
                Phone = user.Phone,
                UserType = user.UserType
            };

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
                TempData["ErrorMessage"] = "Ошибка валидации: " +
                    string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
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
                user.UpdatedAt = DateTime.UtcNow;

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
                return View(model);

            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.PasswordHash))
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
            user.UpdatedAt = DateTime.UtcNow; 
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Пароль успешно изменён";
            return RedirectToAction("Profile");
        }

        [HttpGet]
        public async Task<IActionResult> GetPersonalData()
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _context.Users
                .Include(u => u.IndividualProfile)
                .Include(u => u.PersonalData)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound();

            return Ok(new
            {
                lastName = user.IndividualProfile?.LastName,
                firstName = user.IndividualProfile?.FirstName,
                middleName = user.IndividualProfile?.MiddleName,
                phone = user.Phone,
                residenceAddress = user.PersonalData?.ResidenceAddress,
                inn = user.PersonalData?.Inn,
                passportSeries = user.PersonalData?.PassportSeries,
                passportNumber = user.PersonalData?.PassportNumber,
                passportDate = user.PersonalData?.PassportDate?.ToString("yyyy-MM-dd")
            });
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
            return formType switch
            {
                "Individual" => "INDIVIDUAL",
                "IndividualEntrepreneur" => "ENTREPRENEUR",
                "Organization" => "ORGANIZATION",
                "Representative" => "REPRESENTATIVE",
                _ => "INDIVIDUAL"
            };
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            if (!User.Identity.IsAuthenticated) return Ok(0);

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdClaim, out long userId)) return Ok(0);

            int unreadCount;
            if (User.IsInRole("Admin"))
            {
                unreadCount = await _context.Messages
                    .CountAsync(m => m.IsRead == false && m.Sender.Role != "Admin");
            }
            else
            {
                unreadCount = await (from m in _context.Messages
                                     join a in _context.Applications on m.ApplicationId equals a.Id
                                     where m.IsRead == false && m.SenderId != userId && a.UserId == userId
                                     select m).CountAsync();
            }
            return Ok(unreadCount);
        }
    }
}