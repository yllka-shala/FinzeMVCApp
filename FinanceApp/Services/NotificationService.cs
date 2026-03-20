using ClosedXML.Excel;
using FinanceApp.Data;
using FinanceApp.DTOs;
using FinanceApp.Enums;
using FinanceApp.Helpers;
using FinanceApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FinanceApp.Services
{
    public class NotificationService : INotificationService
    {
        private readonly FinanceAppContext _context;
        private readonly CurrentUser _currentUser;

        public NotificationService(FinanceAppContext context, CurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<PaginatedList<Notification>> GetAll(string? search, int pageNumber = 1, int pageSize = 10)
        {
            var query = _context.Notifications
                                             .Include(nt => nt.NotificationType)
                                             .Include(u => u.User)
                                             .AsNoTracking();

            if (query == null)
                return null;

            if (!_currentUser.IsAdmin())
            {
                int userId = _currentUser.LoggedInUser();
                query = query.Where(u => u.UserId == userId);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p =>
                    p.NotificationType.Name.Contains(search) ||
                    p.Message.Contains(search) ||
                    p.User.FirstName.Contains(search) ||
                    p.User.LastName.Contains(search));
            }

            return await PaginatedList<Notification>.CreateAsync(query, pageNumber, pageSize);
        }

        public async Task<List<Notification>> GetNotifications()
        {
            int userId = _currentUser.LoggedInUser();

            var notification = await _context.Notifications
                                             .Include(nt => nt.NotificationType)
                                             .Include(u => u.User)
                                             .Where(u => u.UserId == userId && u.Read == false)
                                             .ToListAsync();

            if (notification == null)
                return null;

            return notification;
        }

        public async Task<Notification> GetById(int? id)
        {
            var notification = await _context.Notifications
                                             .Include(nt => nt.NotificationType)
                                             .Include(u => u.User).FirstOrDefaultAsync(n => n.Id == id);
            
            if (notification == null)
                return null;

            return notification;
        }

        public async Task<Notification> Create(NotificationDTO model)
        {
            var exists = _context.Notifications.Any(b => b.NotificationTypeId == model.NotificationTypeId && 
                                                         b.UserId == _currentUser.LoggedInUser());
            
            if (exists)
                return null;

            var create = new Notification
            {
                NotificationTypeId = model.NotificationTypeId,
                Message = model.Message,
                Read = model.Read,
                UserId = _currentUser.LoggedInUser()
            };

            await _context.Notifications.AddAsync(create);
            await _context.SaveChangesAsync();

            return create;
        }

        public async Task<Notification> Update(NotificationDTO update, int? id)
        {
            var existingNotification = await _context.Notifications.FindAsync(id);
            if (existingNotification == null)
                return null;

            var exists = _context.Notifications.Any(b => b.NotificationTypeId == update.NotificationTypeId &&
                                                         b.Id != id && b.UserId == _currentUser.LoggedInUser());

            if (exists)
                return null;

            existingNotification.NotificationTypeId = update.NotificationTypeId;
            existingNotification.Message = update.Message;
            existingNotification.Read = update.Read;
            existingNotification.UserId = _currentUser.LoggedInUser();

            await _context.SaveChangesAsync();

            return existingNotification;
        }

        public async Task<Notification> Delete(int? id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
                return null;

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();

            return notification;
        }

        public async Task<List<NotificationType>> GetNotificationTypeList()
        {
            var notificationTypeList = await _context.NotificationTypes.ToListAsync();
            if (notificationTypeList == null)
                return null;

            return notificationTypeList;
        }

        public async Task<List<Notification>> ClearAll()
        {
            var notifications = await _context.Notifications
                                             .Include(nt => nt.NotificationType)
                                             .Include(u => u.User)
                                             .Where(t => t.UserId == _currentUser.LoggedInUser() && t.Read == false)
                                             .ToListAsync();

            if (notifications == null)
                return null;

            foreach (var n in notifications)
            {
                n.Read = true;
            }
            await _context.SaveChangesAsync();

            return notifications;
        }

        public async Task<byte[]> ExportExcel()
        {
            var notifications = await _context.Notifications
                                             .Include(nt => nt.NotificationType)
                                             .Include(u => u.User)
                                             .ToListAsync();

            if (!_currentUser.IsAdmin())
            {
                var userId = _currentUser.LoggedInUser();
                notifications = notifications.Where(t => t.UserId == userId).ToList();
            }

            var exportData = notifications.Select(u => new
            {
                Id = u.Id,
                NotificationType = u.NotificationType!.Name,
                Message = u.Message,
                Read = u.Read,
                CreatedDate = u.CreatedDate.ToString("dd-MM-yyyy"),
                User = $"{u.User!.FirstName} {u.User.LastName}"
            }).ToList();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Notifications");

            worksheet.Cell("A1").InsertTable(exportData);

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

    }
}
