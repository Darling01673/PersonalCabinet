using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalCabinet.Models;
using Rotativa.AspNetCore;
using System.Security.Claims;

namespace PersonalCabinet.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string sortOrder, string statusFilter, string searchString, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.DateSortParam = string.IsNullOrEmpty(sortOrder) ? "date_desc" : "";
            ViewBag.NumberSortParam = sortOrder == "number" ? "number_desc" : "number";
            ViewBag.StatusSortParam = sortOrder == "status" ? "status_desc" : "status";
            IQueryable<Application> query = _context.Applications
                .Include(a => a.User)
                .Where(a => a.Status != "Draft");

            if (!string.IsNullOrEmpty(statusFilter))
                query = query.Where(a => a.Status == statusFilter);

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(a =>
                    a.ApplicationNumber.Contains(searchString) ||
                    a.Title.Contains(searchString) ||
                    a.ObjectAddress.Contains(searchString) ||
                    a.User.Email.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "date_desc":
                    query = query.OrderByDescending(a => a.CreatedAt);
                    break;
                case "number":
                    query = query.OrderBy(a => a.ApplicationNumber);
                    break;
                case "number_desc":
                    query = query.OrderByDescending(a => a.ApplicationNumber);
                    break;
                case "status":
                    query = query.OrderBy(a => a.Status);
                    break;
                case "status_desc":
                    query = query.OrderByDescending(a => a.Status);
                    break;
                default:
                    query = query.OrderByDescending(a => a.CreatedAt);
                    break;
            }

            int pageSize = 10;
            int pageNumber = page ?? 1;
            int totalCount = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.TotalCount = totalCount;
            ViewBag.CurrentPage = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.CurrentStatusFilter = statusFilter;
            ViewBag.CurrentSearchString = searchString;

            return View(items);
        }
        public async Task<IActionResult> Details(long id)
        {
            var application = await _context.Applications
                .Include(a => a.User)
                    .ThenInclude(u => u.IndividualProfile)
                .Include(a => a.User)
                    .ThenInclude(u => u.OrganizationProfile)
                .Include(a => a.User)
                    .ThenInclude(u => u.PersonalData)
                .Include(a => a.Documents)
                .Include(a => a.Messages)
                    .ThenInclude(m => m.Sender)
                        .ThenInclude(s => s.IndividualProfile)
                .Include(a => a.Messages)
                    .ThenInclude(m => m.Sender)
                        .ThenInclude(s => s.OrganizationProfile)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (application == null) return NotFound();

            var currentUserId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var unreadMessages = application.Messages.Where(m => m.IsRead == false && m.SenderId != currentUserId);
            foreach (var msg in unreadMessages)
            {
                msg.IsRead = true;
            }
            await _context.SaveChangesAsync();

            ViewBag.AllStatuses = new List<string> { "Draft", "Submitted", "InReview", "Approved", "Rejected", "Completed" };
            ViewBag.CurrentUserId = currentUserId;
            return View(application);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(long id, string newStatus)
        {
            var app = await _context.Applications.FindAsync(id);
            if (app == null) return NotFound();

            string oldStatus = app.Status;
            app.Status = newStatus;
            app.UpdatedAt = DateTime.Now;

            _context.ApplicationStatusHistories.Add(new ApplicationStatusHistory
            {
                ApplicationId = id,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                ChangedBy = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)),
                CreatedAt = DateTime.Now,
                Comment = $"Статус изменён администратором"
            });
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Статус заявки обновлён";
            return RedirectToAction(nameof(Details), new { id });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(long id, string messageText)
        {
            if (string.IsNullOrWhiteSpace(messageText))
            {
                TempData["ErrorMessage"] = "Сообщение не может быть пустым";
                return RedirectToAction(nameof(Details), new { id });
            }

            var message = new Message
            {
                ApplicationId = id,
                SenderId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)),
                Message1 = messageText,
                IsRead = true,
                CreatedAt = DateTime.Now
            };
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Сообщение отправлено";
            return RedirectToAction(nameof(Details), new { id });
        }
        public async Task<IActionResult> Users(string searchString, int? page)
        {
            IQueryable<User> query = _context.Users
                .Include(u => u.IndividualProfile)
                .Include(u => u.OrganizationProfile);

            if (!string.IsNullOrEmpty(searchString))
                query = query.Where(u => u.Email.Contains(searchString) || u.Phone.Contains(searchString));

            int pageSize = 10;
            int pageNumber = page ?? 1;
            int totalCount = await query.CountAsync();
            var users = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.TotalCount = totalCount;
            ViewBag.CurrentPage = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.CurrentSearchString = searchString;

            return View(users);
        }
        public async Task<IActionResult> UserDetails(long id)
        {
            var user = await _context.Users
                .Include(u => u.IndividualProfile)
                .Include(u => u.OrganizationProfile)
                .Include(u => u.PersonalData)
                .FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();
            return View(user);
        }
        public async Task<IActionResult> Messages()
        {
            var applications = await _context.Applications
                .Include(a => a.Messages)
                .Where(a => a.Messages.Any())
                .Select(a => new
                {
                    a.Id,
                    a.ApplicationNumber,
                    a.Title,
                    LastMessageDate = a.Messages.Max(m => m.CreatedAt)
                })
                .OrderByDescending(a => a.LastMessageDate)
                .ToListAsync();

            ViewBag.Applications = applications;
            return View();
        }
    }
}