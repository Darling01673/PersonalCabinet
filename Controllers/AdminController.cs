using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalCabinet.Models;
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

        public async Task<IActionResult> Index(string sortOrder, string statusFilter, string reasonFilter, string applicantTypeFilter, string searchString, int? page)
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

            if (!string.IsNullOrEmpty(reasonFilter))
                query = query.Where(a => a.ApplicationReason == reasonFilter);

            if (!string.IsNullOrEmpty(applicantTypeFilter))
                query = query.Where(a => a.ApplicantType == applicantTypeFilter);

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(a =>
                    a.ApplicationNumber.Contains(searchString) ||
                    a.Title.Contains(searchString) ||
                    a.ObjectAddress.Contains(searchString) ||
                    a.User.Email.Contains(searchString) ||
                    (a.ApplicantType == "ORGANIZATION" &&
                        (a.OrganizationFullName != null && a.OrganizationFullName.Contains(searchString)) ||
                        (a.OrganizationShortName != null && a.OrganizationShortName.Contains(searchString))));
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
            ViewBag.CurrentReasonFilter = reasonFilter;
            ViewBag.CurrentApplicantTypeFilter = applicantTypeFilter;
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
            app.UpdatedAt = DateTime.UtcNow;

            _context.ApplicationStatusHistories.Add(new ApplicationStatusHistory
            {
                ApplicationId = id,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                ChangedBy = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)),
                CreatedAt = DateTime.UtcNow,
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
                IsRead = false,
                CreatedAt = DateTime.UtcNow
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

        public async Task<IActionResult> Messages(string searchString, bool? unreadOnly, string sortOrder)
        {
            var currentUserId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var query = _context.Applications
                .Where(a => a.Messages.Any() && a.Status != "Draft")
                .Select(a => new
                {
                    a.Id,
                    a.ApplicationNumber,
                    a.Title,
                    a.ApplicantType,
                    LastMessageDate = a.Messages.Max(m => m.CreatedAt),
                    UnreadCount = a.Messages.Count(m => (m.IsRead ?? false) == false && m.SenderId != currentUserId)
                });
            if (!string.IsNullOrEmpty(searchString))
            {
                var appIdsWithText = await _context.Messages
                    .Where(m => m.Message1.Contains(searchString))
                    .Select(m => m.ApplicationId)
                    .Distinct()
                    .ToListAsync();
                query = query.Where(a => appIdsWithText.Contains(a.Id));
            }
            if (unreadOnly == true)
            {
                query = query.Where(a => a.UnreadCount > 0);
            }
            var items = await query.ToListAsync();
            switch (sortOrder)
            {
                case "date_asc":
                    items = items.OrderBy(a => a.LastMessageDate).ToList();
                    break;
                case "date_desc":
                    items = items.OrderByDescending(a => a.LastMessageDate).ToList();
                    break;
                default:
                    items = items.OrderByDescending(a => a.UnreadCount > 0)
                                 .ThenByDescending(a => a.LastMessageDate)
                                 .ToList();
                    break;
            }

            ViewBag.CurrentSearchString = searchString;
            ViewBag.UnreadOnly = unreadOnly ?? false;
            ViewBag.CurrentSort = sortOrder ?? "default";
            ViewBag.Applications = items;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetNewMessages(long id, long lastMessageId = 0)
        {
            var adminId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var newMessages = await _context.Messages
                .Where(m => m.ApplicationId == id && m.Id > lastMessageId && m.SenderId != adminId)
                .Include(m => m.Sender)
                    .ThenInclude(s => s.IndividualProfile)
                .Include(m => m.Sender)
                    .ThenInclude(s => s.OrganizationProfile)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();

            foreach (var msg in newMessages.Where(m => (m.IsRead ?? false) == false))
            {
                msg.IsRead = true;
            }
            await _context.SaveChangesAsync();

            var result = newMessages.Select(m => new
            {
                m.Id,
                m.Message1,
                m.CreatedAt,
                SenderName = GetSenderName(m.Sender),
                IsOwn = false
            });

            return Ok(result);
        }

        private string GetSenderName(User sender)
        {
            if (sender.Role == "Admin") return "Оператор";
            if (sender.UserType == "INDIVIDUAL" && sender.IndividualProfile != null)
                return $"{sender.IndividualProfile.LastName} {sender.IndividualProfile.FirstName} {sender.IndividualProfile.MiddleName}".Trim();
            if (sender.UserType == "ORGANIZATION" && sender.OrganizationProfile != null)
                return sender.OrganizationProfile.ShortName ?? sender.OrganizationProfile.FullName ?? sender.Email;
            return sender.Email;
        }

        public async Task<IActionResult> MainMenuAdmin()
        {
            int submittedCount = await _context.Applications.CountAsync(a => a.Status == "Submitted");
            int inReviewCount = await _context.Applications.CountAsync(a => a.Status == "InReview");
            var currentUserId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            int pendingMessagesCount = await _context.Applications
                .Where(a => a.Messages.Any(m => (m.IsRead ?? false) == false && m.SenderId != currentUserId))
                .CountAsync();
            var latestApplications = await _context.Applications
                .Include(a => a.User)
                .Where(a => a.Status != "Draft")
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .ToListAsync();

            ViewBag.SubmittedCount = submittedCount;
            ViewBag.InReviewCount = inReviewCount;
            ViewBag.PendingMessagesCount = pendingMessagesCount;
            ViewBag.LatestApplications = latestApplications;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetUserPassword([FromBody] ResetPasswordModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.NewPassword))
                return BadRequest(new { message = "Неверные данные" });

            var user = await _context.Users.FindAsync(model.UserId);
            if (user == null)
                return NotFound(new { message = "Пользователь не найден" });
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Пароль успешно изменён" });
        }

        [HttpGet]
        public async Task<IActionResult> Print(long id)
        {
            var application = await _context.Applications
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == id);
            return View(application);
        }

        public class ResetPasswordModel
        {
            public long UserId { get; set; }
            public string NewPassword { get; set; }
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