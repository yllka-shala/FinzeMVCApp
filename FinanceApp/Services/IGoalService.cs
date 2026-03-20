using FinanceApp.DTOs;
using FinanceApp.Helpers;
using FinanceApp.Models;

namespace FinanceApp.Services
{
    public interface IGoalService
    {
        Task<PaginatedList<Goal>> GetAll(string? search, int pageNumber = 1, int pageSize = 10);
        Task<Goal> GetById(int? id);
        Task<Goal> Create(GoalDTO create);
        Task<Goal> Update(GoalDTO update, int? id);
        Task<Goal> Delete(int? id);
        Task<byte[]> ExportExcel();
    }
}
