using FinanceApp.DTOs;
using FinanceApp.Helpers;
using FinanceApp.Models;

namespace FinanceApp.Services
{
    public interface IBillReminderService
    {
        Task<PaginatedList<BillReminder>> GetAll(string? search, int pageNumber = 1, int pageSize = 10);
        Task<BillReminder> GetById(int? id);
        Task<BillReminder> Create(BillReminderDTO create);
        Task<BillReminder> Update(BillReminderDTO update, int? id);
        Task<BillReminder> Delete(int? id);
    }
}
