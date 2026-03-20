using FinanceApp.DTOs;
using FinanceApp.Helpers;
using FinanceApp.Models;

namespace FinanceApp.Services
{
    public interface IRecurringService
    {
        Task<PaginatedList<Recurring>> GetAll(string? search, int pageNumber = 1, int pageSize = 10);
        Task<Recurring> GetById(int? id);
        Task<Recurring> Create(RecurringDTO create);
        Task<Recurring> Update(RecurringDTO update, int? id);
        Task<Recurring> Delete(int? id);
        Task<List<Frequency>> GetFrequenciesList();
        Task<byte[]> ExportExcel();
    }
}
