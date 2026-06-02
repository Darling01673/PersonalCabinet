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
        private readonly ApplicationDbContext _context;

        public ChatHub(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task JoinApplicationGroup(long applicationId)
        {
            string groupName = GetGroupName(applicationId);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }
        public async Task LeaveApplicationGroup(long applicationId)
        {
            string groupName = GetGroupName(applicationId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }
        public async Task SendMessage(long applicationId, string messageText)
        {
            if (string.IsNullOrWhiteSpace(messageText))
                return;

            long userId = long.Parse(Context.User.FindFirstValue(ClaimTypes.NameIdentifier));
            User sender = await _context.Users
                .Include(u => u.IndividualProfile)
                .Include(u => u.OrganizationProfile)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (sender == null) return;

            string senderName = GetSenderName(sender);
            var message = new Message
            {
                ApplicationId = applicationId,
                SenderId = userId,
                Message1 = messageText,
                CreatedAt = DateTime.Now
            };
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            string groupName = GetGroupName(applicationId);
            var messageForOthers = new
            {
                id = message.Id,
                senderName = senderName,
                messageText = messageText,
                createdAt = message.CreatedAt.Value.ToString("dd.MM.yyyy HH:mm"),
                isOwn = false
            };
            var messageForCaller = new
            {
                id = message.Id,
                senderName = senderName,
                messageText = messageText,
                createdAt = message.CreatedAt.Value.ToString("dd.MM.yyyy HH:mm"),
                isOwn = true
            };
            await Clients.OthersInGroup(groupName).SendAsync("ReceiveMessage", messageForOthers);
            await Clients.Caller.SendAsync("ReceiveMessage", messageForCaller);
        }

        private string GetGroupName(long applicationId) => $"application_{applicationId}";

        private string GetSenderName(User user)
        {
            if (user.Role == "Admin")
            {
                return user.IndividualProfile != null
                    ? $"Оператор {user.IndividualProfile.LastName} {user.IndividualProfile.FirstName} {user.IndividualProfile.MiddleName}".Trim()
                    : "Оператор";
            }

            if (user.UserType == "INDIVIDUAL" && user.IndividualProfile != null)
            {
                return $"{user.IndividualProfile.LastName} {user.IndividualProfile.FirstName} {user.IndividualProfile.MiddleName}".Trim();
            }
            else if (user.UserType == "ORGANIZATION" && user.OrganizationProfile != null)
            {
                return user.OrganizationProfile.ShortName ?? user.OrganizationProfile.FullName ?? user.Email;
            }

            return user.Email;
        }
    }
}