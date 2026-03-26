using FinanceApp.DTOs;
using FinanceApp.Helpers;
using FinanceApp.Models;

namespace FinanceApp.Services
{
    public interface ICategoryService
    {
        Task<PaginatedList<CategoryWithUserDto>> GetAll(string? search, int pageNumber = 1, int pageSize = 10);
        Task<List<CategoryWithUserDto>> GetCategories();
        Task<Category> GetById(int? id);
        Task<Category> Create(CategoryDTO create);
        Task<Category> Update(CategoryDTO category, int? id);
        Task<Category> Delete(int? id);
        Task<byte[]> ExportExcel();
        Task<byte[]> ExportPdf();
    }
}
