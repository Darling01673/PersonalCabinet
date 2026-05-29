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

        public async Task<IActionResult> Index()
        {
            string userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Challenge();
            }
            long userId = long.Parse(userIdClaim);

            List<Application> applicationsWithMessages = await _context.Applications
                .Include(a => a.Messages)
                .Where(a => a.UserId == userId && a.Messages.Any())
                .ToListAsync();

            List<object> resultList = new List<object>();

            foreach (Application app in applicationsWithMessages)
            {
                DateTime? lastMessageDate = null;
                foreach (Message msg in app.Messages)
                {
                    if (lastMessageDate == null || msg.CreatedAt > lastMessageDate)
                    {
                        lastMessageDate = msg.CreatedAt;
                    }
                }

                int unreadCount = 0;
                foreach (Message msg in app.Messages)
                {
                    if (msg.IsRead == false && msg.SenderId != userId)
                    {
                        unreadCount++;
                    }
                }

                var item = new
                {
                    app.Id,
                    app.ApplicationNumber,
                    app.Title,
                    LastMessageDate = lastMessageDate,
                    UnreadCount = unreadCount
                };

                resultList.Add(item);
            }

            resultList = resultList.OrderByDescending(item => item.GetType().GetProperty("LastMessageDate").GetValue(item, null)).ToList();

            ViewBag.Applications = resultList;
            return View();
        }
    }
}