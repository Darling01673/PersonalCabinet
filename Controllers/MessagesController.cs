using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalCabinet.Models;
using System.Security.Claims;

namespace PersonalCabinet.Controllers
{
    [Authorize]
    public class MessagesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MessagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString, bool? unreadOnly, string sortOrder)
        {
            string userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Challenge();
            long userId = long.Parse(userIdClaim);
            var applications = await _context.Applications
                .Include(a => a.Messages)
                .Where(a => a.UserId == userId)
                .ToListAsync();
            List<object> resultList = new List<object>();

            foreach (var app in applications)
            {
                if (app.Messages == null || app.Messages.Count == 0)
                    continue;
                if (!string.IsNullOrEmpty(searchString))
                {
                    bool found = false;
                    foreach (var msg in app.Messages)
                    {
                        if (msg.Message1 != null && msg.Message1.Contains(searchString))
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found) continue;
                }
                DateTime? lastDate = null;
                foreach (var msg in app.Messages)
                {
                    if (lastDate == null || msg.CreatedAt > lastDate)
                        lastDate = msg.CreatedAt;
                }
                int unread = 0;
                foreach (var msg in app.Messages)
                {
                    if (msg.IsRead == false && msg.SenderId != userId)
                        unread++;
                }
                resultList.Add(new
                {
                    app.Id,
                    app.ApplicationNumber,
                    app.Title,
                    LastMessageDate = lastDate,
                    UnreadCount = unread
                });
            }

            if (unreadOnly == true)
            {
                List<object> filtered = new List<object>();
                foreach (var item in resultList)
                {
                    dynamic d = item;
                    if (d.UnreadCount > 0)
                        filtered.Add(item);
                }
                resultList = filtered;
            }

            if (sortOrder == "date_asc")
            {
                resultList = resultList.OrderBy(x => ((dynamic)x).LastMessageDate).ToList();
            }
            else if (sortOrder == "date_desc")
            {
                resultList = resultList.OrderByDescending(x => ((dynamic)x).LastMessageDate).ToList();
            }
            else
            {
                resultList = resultList
                    .OrderByDescending(x => ((dynamic)x).UnreadCount > 0)
                    .ThenByDescending(x => ((dynamic)x).LastMessageDate)
                    .ToList();
            }
            ViewBag.CurrentSearchString = searchString;
            ViewBag.UnreadOnly = unreadOnly ?? false;
            ViewBag.CurrentSort = sortOrder ?? "default";
            ViewBag.Applications = resultList;

            return View();
        }
    }
}