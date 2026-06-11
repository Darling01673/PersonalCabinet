using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalCabinet.Models;
using System.Diagnostics;
using System.Security.Claims;

namespace PersonalCabinet.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
        [AllowAnonymous]
        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Admin"))
                    return RedirectToAction("MainMenuAdmin", "Admin");
                else 
                    return RedirectToAction("MainMenu");
            }
            return View();
        }

        public async Task<IActionResult> MainMenu()
        {
            if (User.IsInRole("Admin"))
                return RedirectToAction("MainMenuAdmin", "Admin");
            string userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            long userId = long.Parse(userIdString);

            User user = await _context.Users
                .Include(u => u.IndividualProfile)
                .Include(u => u.OrganizationProfile)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return Challenge();
            }

            List<Application> applications = await _context.Applications
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .ToListAsync();

            List<Message> messages = await _context.Messages
                .Include(m => m.Sender)
                .Where(m => m.Application.UserId == userId && m.IsRead != true)
                .OrderByDescending(m => m.CreatedAt)
                .Take(5)
                .ToListAsync();

            ViewBag.User = user;
            ViewBag.UserApplications = applications;
            ViewBag.UnreadMessages = messages;

            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
            }

            return View();
        }

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            ErrorViewModel model = new ErrorViewModel();
            model.RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            return View(model);
        }
        [AllowAnonymous]
        public IActionResult Instruction()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult About()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Faq()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Contacts()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Documents()
        {
            return View();
        }
        public IActionResult RateLimit(int seconds)
        {
            ViewBag.Seconds = seconds;
            return View();
        }
    }
}