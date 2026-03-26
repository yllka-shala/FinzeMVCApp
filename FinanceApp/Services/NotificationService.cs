using ClosedXML.Excel;
using FinanceApp.Data;
using FinanceApp.DTOs;
using FinanceApp.Enums;
using FinanceApp.Helpers;
using FinanceApp.Models;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
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
            var exportData = await GetNotificationExportData();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Notifications");

            worksheet.Cell("A1").InsertTable(exportData);

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> ExportPdf()
        {
            var exportData = await GetNotificationExportData();

            QuestPDF.Settings.License = LicenseType.Community;

            // Create the document
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header().Text("Notification Report").FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);

                    page.Content().PaddingVertical(30).Table(table =>
                    {
                        // Define columns
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(50);
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        // Add Header
                        table.Header(header =>
                        {
                            header.Cell().BorderBottom(2).Padding(3).Text("Id").Bold();
                            header.Cell().BorderBottom(2).Padding(3).Text("NotificationType").Bold();
                            header.Cell().BorderBottom(2).Padding(3).Text("Message").Bold();
                            header.Cell().BorderBottom(2).Padding(3).Text("Read").Bold();
                            header.Cell().BorderBottom(2).Padding(3).Text("CreatedDate").Bold();
                            header.Cell().BorderBottom(2).Padding(3).Text("User").Bold();
                        });

                        // Add Data Rows
                        foreach (var item in exportData)
                        {
                            table.Cell().Padding(3).Text(item.Id.ToString());
                            table.Cell().Padding(3).Text(item.NotificationType);
                            table.Cell().Padding(3).Text(item.Message);
                            table.Cell().Padding(3).Text(item.Read);
                            table.Cell().Padding(3).Text(item.CreatedDate);
                            table.Cell().Padding(3).Text(item.User);
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                    });
                });
            });

            return document.GeneratePdf();
        }

        private async Task<List<ExportNotificationDataDTO>> GetNotificationExportData()
        {
            var query = _context.Notifications
                                .Include(nt => nt.NotificationType)
                                .Include(u => u.User)
                                .AsQueryable();


            if (!_currentUser.IsAdmin())
            {
                var userId = _currentUser.LoggedInUser();
                query = query.Where(t => t.UserId == userId);
            }

            var notifications = await query.ToListAsync();

            return notifications.Select(u => new ExportNotificationDataDTO
            {
                Id = u.Id,
                NotificationType = u.NotificationType!.Name,
                Message = u.Message,
                Read = u.Read.ToString(),
                CreatedDate = u.CreatedDate.ToString("dd-MM-yyyy"),
                User = $"{u.User!.FirstName} {u.User.LastName}"
            }).ToList();
        }

    }
}
