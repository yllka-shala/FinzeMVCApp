using FinanceApp.DTOs;
using FinanceApp.Helpers;
using FinanceApp.Models;

namespace FinanceApp.Services
{
    public interface IBudgetService
    {
        Task<PaginatedList<Budget>> GetAll(string? search, int pageNumber = 1, int pageSize = 10);
        Task<Budget> GetById(int? id);
        Task<Budget> Create(BudgetDTO budget);
        Task<Budget> Update(BudgetDTO budget, int? id);
        Task<Budget> Delete(int? id);
        Task<byte[]> ExportExcel();
        Task<byte[]> ExportPdf();
    }
}
