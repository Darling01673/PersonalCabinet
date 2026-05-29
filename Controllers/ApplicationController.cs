using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalCabinet.Models;
using PersonalCabinet.Services;
using PersonalCabinet.ViewModels;

namespace PersonalCabinet.Controllers
{
    [Authorize]
    public class ApplicationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly FileService _fileService;

        public ApplicationController(ApplicationDbContext context, FileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        public async Task<IActionResult> Index(string sortOrder, string statusFilter, string searchString, int? page)
        {
            long? userId = GetCurrentUserId();
            if (userId == null) return Challenge();

            IQueryable<Application> query = _context.Applications;
            if (!User.IsInRole("Admin"))
                query = query.Where(a => a.UserId == userId);
            else
                query = query.Include(a => a.User);

            if (!string.IsNullOrEmpty(statusFilter))
                query = query.Where(a => a.Status == statusFilter);

            if (!string.IsNullOrEmpty(searchString))
                query = query.Where(a =>
                    a.ApplicationNumber.Contains(searchString) ||
                    a.Title.Contains(searchString) ||
                    a.ObjectAddress.Contains(searchString));

            if (sortOrder == "number")
                query = query.OrderBy(a => a.ApplicationNumber);
            else if (sortOrder == "number_desc")
                query = query.OrderByDescending(a => a.ApplicationNumber);
            else if (sortOrder == "status")
                query = query.OrderBy(a => a.Status);
            else if (sortOrder == "status_desc")
                query = query.OrderByDescending(a => a.Status);
            else
                query = query.OrderByDescending(a => a.CreatedAt);

            int pageSize = 10;
            int pageNumber = page ?? 1;
            int totalCount = await query.CountAsync();
            List<Application> items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.TotalCount = totalCount;
            ViewBag.CurrentPage = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.CurrentStatusFilter = statusFilter;
            ViewBag.CurrentSearchString = searchString;

            return View(items);
        }

        [HttpGet]
        public async Task<IActionResult> Details(long id)
        {
            long? userId = GetCurrentUserId();
            if (userId == null) return Challenge();

            Application application = await _context.Applications
                .Include(a => a.Documents)
                .Include(a => a.Messages)
                    .ThenInclude(m => m.Sender)
                        .ThenInclude(s => s.IndividualProfile) 
                .Include(a => a.Messages)
                    .ThenInclude(m => m.Sender)
                        .ThenInclude(s => s.OrganizationProfile) 
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (application == null) return NotFound();
            if (application.UserId != userId && !User.IsInRole("Admin")) return NotFound();

            ViewBag.CurrentUserId = userId;
            return View(application);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var model = new CreateApplicationViewModel();
            model.PassportDate = DateTime.Now;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateApplicationViewModel model)
        {
            long? userId = GetCurrentUserId();
            if (userId == null) return Challenge();

            ModelState.Remove("Attachments");
            if (!ModelState.IsValid)
                return View(model);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var application = new Application
                {
                    UserId = userId.Value,
                    Title = $"Заявка от {DateTime.Now:dd.MM.yyyy}",
                    ObjectAddress = model.DeviceAddress,
                    RequestedPower = model.RequestedPower,
                    Status = "Draft",
                    ApplicationNumber = GenerateApplicationNumber(),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    EnergyDeviceName = model.EnergyDeviceName,
                    DeviceAddress = model.DeviceAddress,
                    PreviousPowerKw = model.PreviousPowerKw,
                    TotalPowerKw = model.TotalPowerKw,
                    ReliabilityCategory = model.ReliabilityCategory,
                    DesignDeadline = model.DesignDeadline,
                    ApplicationReason = model.ApplicationReason,
                    LastName = model.LastName,
                    FirstName = model.FirstName,
                    MiddleName = model.MiddleName,
                    ResidenceAddress = model.ResidenceAddress,
                    Phone = model.Phone,
                    Inn = model.Inn,
                    PassportSeries = model.PassportSeries,
                    PassportNumber = model.PassportNumber,
                    PassportDate = model.PassportDate
                };
                _context.Applications.Add(application);
                await _context.SaveChangesAsync();

                if (model.Attachments != null && model.Attachments.Any())
                {
                    foreach (var file in model.Attachments)
                    {
                        if (file.Length == 0) continue;
                        var (storedName, relativePath) = await _fileService.SaveFileAsync(file, application.Id);
                        _context.Documents.Add(new Document
                        {
                            ApplicationId = application.Id,
                            OriginalFileName = file.FileName,
                            StoredFileName = storedName,
                            FilePath = relativePath,
                            MimeType = file.ContentType,
                            UploadedBy = userId,
                            UploadedAt = DateTime.Now
                        });
                    }
                    await _context.SaveChangesAsync();
                }

                string submitType = Request.Form["submitType"];
                if (submitType == "submit")
                {
                    application.Status = "Submitted";
                    application.SubmittedAt = DateTime.Now;
                    _context.ApplicationStatusHistories.Add(new ApplicationStatusHistory
                    {
                        ApplicationId = application.Id,
                        OldStatus = "Draft",
                        NewStatus = "Submitted",
                        ChangedBy = userId,
                        CreatedAt = DateTime.Now,
                        Comment = "Заявка подана через форму создания"
                    });
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Заявка успешно подана!";
                }
                else
                {
                    TempData["SuccessMessage"] = "Черновик сохранён.";
                }

                await transaction.CommitAsync();
                return RedirectToAction("Details", new { id = application.Id });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", "Ошибка: " + ex.Message);
                if (ex.InnerException != null)
                    ModelState.AddModelError("", "Внутренняя: " + ex.InnerException.Message);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(long id)
        {
            long? userId = GetCurrentUserId();
            if (userId == null) return Challenge();

            var application = await _context.Applications
                .Include(a => a.Documents)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (application == null) return NotFound();
            if (application.UserId != userId && !User.IsInRole("Admin")) return NotFound();
            if (application.Status != "Draft" && application.Status != "Rejected")
            {
                TempData["ErrorMessage"] = "Редактирование доступно только для черновиков или отклонённых заявок.";
                return RedirectToAction("Details", new { id });
            }
            return View(application);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, Application model, string submitType)
        {
            if (id != model.Id) return BadRequest();

            long? userId = GetCurrentUserId();
            if (userId == null) return Challenge();

            var application = await _context.Applications.FindAsync(id);
            if (application == null) return NotFound();
            if (application.UserId != userId && !User.IsInRole("Admin")) return NotFound();

            if (application.Status != "Draft" && application.Status != "Rejected")
            {
                TempData["ErrorMessage"] = "Редактирование недоступно.";
                return RedirectToAction("Details", new { id });
            }
            application.Title = model.Title;
            application.Description = model.Description;
            application.ObjectAddress = model.ObjectAddress;
            application.RequestedPower = model.RequestedPower;
            application.UpdatedAt = DateTime.Now;
            application.ApplicationReason = model.ApplicationReason;
            application.EnergyDeviceName = model.EnergyDeviceName;
            application.DeviceAddress = model.DeviceAddress;
            application.PreviousPowerKw = model.PreviousPowerKw;
            application.TotalPowerKw = model.TotalPowerKw;
            application.ReliabilityCategory = model.ReliabilityCategory;
            application.DesignDeadline = model.DesignDeadline;
            application.LastName = model.LastName;
            application.FirstName = model.FirstName;
            application.MiddleName = model.MiddleName;
            application.ResidenceAddress = model.ResidenceAddress;
            application.Phone = model.Phone;
            application.Inn = model.Inn;
            application.PassportSeries = model.PassportSeries;
            application.PassportNumber = model.PassportNumber;
            application.PassportDate = model.PassportDate;

            if (submitType == "submit")
            {
                application.Status = "Submitted";
                application.SubmittedAt = DateTime.Now;
                _context.ApplicationStatusHistories.Add(new ApplicationStatusHistory
                {
                    ApplicationId = id,
                    OldStatus = "Draft",
                    NewStatus = "Submitted",
                    ChangedBy = userId,
                    CreatedAt = DateTime.Now,
                    Comment = "Заявка отправлена на рассмотрение через редактирование"
                });
                TempData["SuccessMessage"] = "Заявка отправлена на рассмотрение!";
            }
            else
            {
                TempData["SuccessMessage"] = "Черновик сохранён.";
            }

            if (application.Status == "Rejected") application.Status = "Draft";

            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddDocument(long id, IFormFile file, long? documentTypeId = null)
        {
            long? userId = GetCurrentUserId();
            if (userId == null) return Challenge();

            var application = await _context.Applications.FindAsync(id);
            if (application == null) return NotFound();
            if (application.UserId != userId && !User.IsInRole("Admin")) return NotFound();

            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Выберите файл.";
                return RedirectToAction("Edit", new { id });
            }
            if (file.Length > 10 * 1024 * 1024)
            {
                TempData["ErrorMessage"] = "Файл не должен превышать 10 МБ.";
                return RedirectToAction("Edit", new { id });
            }

            var (storedName, relativePath) = await _fileService.SaveFileAsync(file, id);
            if (string.IsNullOrEmpty(relativePath))
            {
                TempData["ErrorMessage"] = "Ошибка при сохранении файла.";
                return RedirectToAction("Edit", new { id });
            }

            var document = new Document
            {
                ApplicationId = id,
                DocumentTypeId = documentTypeId,
                OriginalFileName = file.FileName,
                StoredFileName = storedName,
                FilePath = relativePath,
                MimeType = file.ContentType,
                UploadedBy = userId,
                UploadedAt = DateTime.Now
            };
            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Файл загружен.";
            return RedirectToAction("Edit", new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDocument(long documentId)
        {
            long? userId = GetCurrentUserId();
            if (userId == null) return Challenge();

            var document = await _context.Documents.Include(d => d.Application).FirstOrDefaultAsync(d => d.Id == documentId);
            if (document == null) return NotFound();

            var application = document.Application;
            if (application.UserId != userId && !User.IsInRole("Admin")) return NotFound();

            _fileService.DeleteFile(document.FilePath);
            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Файл удалён.";
            return RedirectToAction("Edit", new { id = application.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(long id)
        {
            long? userId = GetCurrentUserId();
            if (userId == null) return Challenge();

            var application = await _context.Applications.FindAsync(id);
            if (application == null) return NotFound();
            if (application.UserId != userId && !User.IsInRole("Admin")) return NotFound();
            if (application.Status != "Draft")
            {
                TempData["ErrorMessage"] = "Отправить можно только черновик.";
                return RedirectToAction("Details", new { id });
            }

            application.Status = "Submitted";
            application.SubmittedAt = DateTime.Now;
            application.UpdatedAt = DateTime.Now;

            _context.ApplicationStatusHistories.Add(new ApplicationStatusHistory
            {
                ApplicationId = id,
                OldStatus = "Draft",
                NewStatus = "Submitted",
                ChangedBy = userId,
                CreatedAt = DateTime.Now,
                Comment = "Заявка отправлена на рассмотрение"
            });
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Заявка отправлена на рассмотрение.";
            return RedirectToAction("Details", new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(long id, string messageText)
        {
            long? userId = GetCurrentUserId();
            if (userId == null) return Challenge();

            var application = await _context.Applications.FindAsync(id);
            if (application == null) return NotFound();
            if (application.UserId != userId && !User.IsInRole("Admin")) return NotFound();
            if (string.IsNullOrWhiteSpace(messageText))
            {
                TempData["ErrorMessage"] = "Сообщение не может быть пустым.";
                return RedirectToAction("Details", new { id });
            }

            var message = new Message
            {
                ApplicationId = id,
                SenderId = userId.Value,
                Message1 = messageText,
                IsRead = false,
                CreatedAt = DateTime.Now
            };
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Сообщение отправлено.";
            return RedirectToAction("Details", new { id });
        }

        [HttpGet]
        public async Task<IActionResult> Delete(long id)
        {
            long? userId = GetCurrentUserId();
            if (userId == null) return Challenge();

            var application = await _context.Applications.Include(a => a.Documents).FirstOrDefaultAsync(a => a.Id == id);
            if (application == null) return NotFound();
            if (application.UserId != userId && !User.IsInRole("Admin")) return NotFound();
            if (application.Status != "Draft" && application.Status != "Rejected")
            {
                TempData["ErrorMessage"] = "Удалить можно только черновик или отклонённую заявку.";
                return RedirectToAction("Details", new { id });
            }
            return View(application);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            long? userId = GetCurrentUserId();
            if (userId == null) return Challenge();

            var application = await _context.Applications
                .Include(a => a.Documents)
                .Include(a => a.ApplicationStatusHistories)
                .Include(a => a.Messages)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (application == null) return NotFound();
            if (application.UserId != userId && !User.IsInRole("Admin")) return NotFound();
            if (application.Status != "Draft" && application.Status != "Rejected")
            {
                TempData["ErrorMessage"] = "Нельзя удалить заявку в текущем статусе.";
                return RedirectToAction("Details", new { id });
            }

            foreach (var doc in application.Documents)
                _fileService.DeleteFile(doc.FilePath);
            _context.Applications.Remove(application);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Заявка удалена.";
            return RedirectToAction("Index");
        }

        private long? GetCurrentUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(claim)) return null;
            return long.TryParse(claim, out long id) ? id : null;
        }

        private string GenerateApplicationNumber()
        {
            int year = DateTime.Now.Year;
            int seq = 1;
            string number;
            do
            {
                number = $"ТП-{year}-{seq:D6}";
                seq++;
            } while (_context.Applications.Any(a => a.ApplicationNumber == number));
            return number;
        }
        [HttpGet]
        public async Task<IActionResult> GetNewMessages(long id, long lastMessageId = 0)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var application = await _context.Applications.FindAsync(id);
            if (application == null || application.UserId != userId) return NotFound();

            var newMessages = await _context.Messages
                .Where(m => m.ApplicationId == id && m.Id > lastMessageId)
                .Include(m => m.Sender)
                    .ThenInclude(s => s.IndividualProfile)
                .Include(m => m.Sender)
                    .ThenInclude(s => s.OrganizationProfile)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();

            foreach (var msg in newMessages.Where(m => m.SenderId != userId && (m.IsRead ?? false) == false))
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
                IsOwn = m.SenderId == userId
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
    }
}