using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PersonalCabinet.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PersonalCabinet.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _db;

        public ChatHub(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task SubscribeToNotifications()
        {
            var userId = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                string group = "notifications_" + userId;
                await Groups.AddToGroupAsync(Context.ConnectionId, group);
            }
        }

        public async Task UnsubscribeFromNotifications()
        {
            var userId = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                string group = "notifications_" + userId;
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);
            }
        }
        public async Task JoinApplicationGroup(long appId)
        {
            string group = "app_" + appId;
            await Groups.AddToGroupAsync(Context.ConnectionId, group);
        }

        public async Task LeaveApplicationGroup(long appId)
        {
            string group = "app_" + appId;
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);
        }

        public async Task SendMessage(long appId, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;
            var userIdStr = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            long userId = long.Parse(userIdStr);
            var app = await _db.Applications.FirstOrDefaultAsync(a => a.Id == appId);
            if (app == null) return;
            if (app.UserId != userId && !Context.User.IsInRole("Admin"))
                return;
            var sender = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (sender == null) return;

            string senderName = "";
            if (sender.Role == "Admin")
            {
                var profile = await _db.IndividualProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
                if (profile != null)
                    senderName = "Оператор " + profile.LastName;
                else
                    senderName = "Оператор";
            }
            else if (sender.UserType == "INDIVIDUAL")
            {
                var profile = await _db.IndividualProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
                if (profile != null)
                    senderName = profile.LastName + " " + profile.FirstName;
                else
                    senderName = sender.Email;
            }
            else
            {
                var profile = await _db.OrganizationProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
                if (profile != null)
                    senderName = profile.ShortName ?? profile.FullName;
                else
                    senderName = sender.Email;
            }
            var msg = new Message
            {
                ApplicationId = appId,
                SenderId = userId,
                Message1 = text,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };
            _db.Messages.Add(msg);
            await _db.SaveChangesAsync();
            string createdAtString = msg.CreatedAt?.ToString("dd.MM.yyyy HH:mm")
                         ?? DateTime.UtcNow.ToString("dd.MM.yyyy HH:mm");
            var dto = new
            {
                id = msg.Id,
                senderName = senderName,
                messageText = text,
                createdAt = createdAtString,
                isOwn = false
            };
            var dtoSelf = new
            {
                id = msg.Id,
                senderName = senderName,
                messageText = text,
                createdAt = createdAtString,
                isOwn = true
            };
            string groupName = "app_" + appId;
            await Clients.OthersInGroup(groupName).SendAsync("ReceiveMessage", dto);
            await Clients.Caller.SendAsync("ReceiveMessage", dtoSelf);

            if (Context.User.IsInRole("Admin"))
            {
                await Clients.Group("notifications_" + app.UserId).SendAsync("UpdateNotificationCount");
            }
            else
            {
                var admins = await _db.Users.Where(u => u.Role == "Admin").ToListAsync();
                foreach (var admin in admins)
                {
                    await Clients.Group("notifications_" + admin.Id).SendAsync("UpdateNotificationCount");
                }
            }
        }
    }
}