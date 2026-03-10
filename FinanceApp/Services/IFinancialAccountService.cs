using FinanceApp.DTOs;
using FinanceApp.Helpers;
using FinanceApp.Models;

namespace FinanceApp.Services
{
    public interface IFinancialAccountService
    {
        Task<PaginatedList<FinancialAccount>> GetAll(string? search, int pageNumber = 1, int pageSize = 10);
        Task<FinancialAccount> GetById(int? id);
        Task<FinancialAccount> Create(FinancialAccountDTO create);
        Task<FinancialAccount> Update(FinancialAccountDTO update, int? id);
        Task<FinancialAccount> Delete(int? id);
        Task<List<AccountType>> GetAccountTypeList();
    }
}
