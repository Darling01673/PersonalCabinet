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

        public async Task SubscribeToNotifications()
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                string groupName = $"notifications_{userId}";
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            }
        }

        public async Task UnsubscribeFromNotifications()
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                string groupName = $"notifications_{userId}";
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            }
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
            try
            {
                if (string.IsNullOrWhiteSpace(messageText))
                {
                    await Clients.Caller.SendAsync("ReceiveError", "Сообщение не может быть пустым");
                    return;
                }

                var userIdClaim = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out long userId))
                {
                    await Clients.Caller.SendAsync("ReceiveError", "Не удалось определить пользователя");
                    return;
                }

                bool hasAccess = await _context.Applications
                    .AnyAsync(a => a.Id == applicationId && (a.UserId == userId || Context.User.IsInRole("Admin")));
                if (!hasAccess)
                {
                    await Clients.Caller.SendAsync("ReceiveError", "Нет доступа к этой заявке");
                    return;
                }

                var sender = await _context.Users
                    .Include(u => u.IndividualProfile)
                    .Include(u => u.OrganizationProfile)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (sender == null)
                {
                    await Clients.Caller.SendAsync("ReceiveError", "Пользователь не найден");
                    return;
                }

                string senderName = GetSenderName(sender);
                var message = new Message
                {
                    ApplicationId = applicationId,
                    SenderId = userId,
                    Message1 = messageText,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };

                _context.Messages.Add(message);
                await _context.SaveChangesAsync();

                string groupName = GetGroupName(applicationId);
                var formattedDate = message.CreatedAt?.ToString("dd.MM.yyyy HH:mm") ?? DateTime.UtcNow.ToString("dd.MM.yyyy HH:mm");

                var messageForOthers = new
                {
                    id = message.Id,
                    senderName,
                    messageText,
                    createdAt = formattedDate,
                    isOwn = false
                };
                var messageForCaller = new
                {
                    id = message.Id,
                    senderName,
                    messageText,
                    createdAt = formattedDate,
                    isOwn = true
                };

                await Clients.OthersInGroup(groupName).SendAsync("ReceiveMessage", messageForOthers);
                await Clients.Caller.SendAsync("ReceiveMessage", messageForCaller);
                long recipientUserId;
                if (Context.User.IsInRole("Admin"))
                {
                    var application = await _context.Applications.FindAsync(applicationId);
                    recipientUserId = application.UserId;
                }
                else
                {
                    var adminIds = await _context.Users
                        .Where(u => u.Role == "Admin")
                        .Select(u => u.Id)
                        .ToListAsync();
                    foreach (var adminId in adminIds)
                    {
                        await Clients.Group($"notifications_{adminId}").SendAsync("UpdateNotificationCount");
                    }
                    return;
                }
                await Clients.Group($"notifications_{recipientUserId}").SendAsync("UpdateNotificationCount");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SignalR] Ошибка в SendMessage: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                await Clients.Caller.SendAsync("ReceiveError", "Внутренняя ошибка сервера при отправке сообщения");
                throw new HubException("Не удалось отправить сообщение: " + ex.Message);
            }
        }

        private string GetGroupName(long applicationId) => $"application_{applicationId}";

        private string GetSenderName(User user)
        {
            if (user == null) return "Неизвестный";

            if (user.Role == "Admin")
            {
                if (user.IndividualProfile != null)
                {
                    return $"Оператор {user.IndividualProfile.LastName} {user.IndividualProfile.FirstName} {user.IndividualProfile.MiddleName}".Trim();
                }
                return "Оператор";
            }

            if (user.UserType == "INDIVIDUAL" && user.IndividualProfile != null)
            {
                return $"{user.IndividualProfile.LastName} {user.IndividualProfile.FirstName} {user.IndividualProfile.MiddleName}".Trim();
            }

            if (user.UserType == "ORGANIZATION" && user.OrganizationProfile != null)
            {
                return user.OrganizationProfile.ShortName ?? user.OrganizationProfile.FullName ?? user.Email;
            }

            return user.Email ?? "Пользователь";
        }
    }
}