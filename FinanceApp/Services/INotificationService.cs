using FinanceApp.DTOs;
using FinanceApp.Helpers;
using FinanceApp.Models;

namespace FinanceApp.Services
{
    public interface INotificationService
    {
        Task<PaginatedList<Notification>> GetAll(string? search, int pageNumber = 1, int pageSize = 10);
        Task<List<Notification>> GetNotifications();
        Task<Notification> GetById(int? id);
        Task<Notification> Create(NotificationDTO create);
        Task<Notification> Update(NotificationDTO update, int? id);
        Task<Notification> Delete(int? id);
        Task<List<NotificationType>> GetNotificationTypeList();
        Task<List<Notification>> ClearAll();
        Task<byte[]> ExportExcel();
        Task<byte[]> ExportPdf();
    }
}
