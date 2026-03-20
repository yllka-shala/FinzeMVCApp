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
    public class FinancialAccountService : IFinancialAccountService
    {
        private readonly FinanceAppContext _context;
        private readonly CurrentUser _currentUser;

        public FinancialAccountService(FinanceAppContext context, CurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<PaginatedList<FinancialAccount>> GetAll(string? search, int pageNumber = 1, int pageSize = 10)
        {
            var query = _context.FinancialAccounts
                                                .Include(fa => fa.AccountType)
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
                    p.AccountType.Name.Contains(search) ||
                    p.Bank.Contains(search) ||
                    p.User.FirstName.Contains(search) ||
                    p.User.LastName.Contains(search));
            }

            return await PaginatedList<FinancialAccount>.CreateAsync(query, pageNumber, pageSize);
        }

        public async Task<FinancialAccount> GetById(int? id)
        {
            var financialAccount = await _context.FinancialAccounts
                                                 .Include(fa => fa.AccountType)
                                                 .Include(u => u.User).FirstOrDefaultAsync(fa => fa.Id == id);
            
            if (financialAccount == null)
                return null;

            return financialAccount;
        }

        public async Task<FinancialAccount> Create(FinancialAccountDTO model)
        {
            var create = new FinancialAccount
            {
                AccountTypeId = model.AccountTypeId,
                AccountBalance = model.AccountBalance,
                Bank = model.Bank,
                UserId = _currentUser.LoggedInUser()
            };

            await _context.FinancialAccounts.AddAsync(create);
            await _context.SaveChangesAsync();

            return create;
        }

        public async Task<FinancialAccount> Update(FinancialAccountDTO update, int? id)
        {
            var existingFinancialAccounts = await _context.FinancialAccounts.FindAsync(id);
            if (existingFinancialAccounts == null)
                return null;

            existingFinancialAccounts.AccountTypeId = update.AccountTypeId;
            existingFinancialAccounts.AccountBalance = update.AccountBalance;
            existingFinancialAccounts.Bank = update.Bank;
            existingFinancialAccounts.UserId = _currentUser.LoggedInUser();

            await _context.SaveChangesAsync();

            return existingFinancialAccounts;
        }

        public async Task<FinancialAccount> Delete(int? id)
        {
            var financialAccount = await _context.FinancialAccounts.FindAsync(id);
            if (financialAccount == null)
                return null;

            _context.FinancialAccounts.Remove(financialAccount);
            await _context.SaveChangesAsync();

            return financialAccount;
        }

        public async Task<List<AccountType>> GetAccountTypeList()
        {
            var accountType = await _context.AccountTypes.ToListAsync();
            if (accountType == null)
                return null;

            return accountType;
        }

        public async Task<byte[]> ExportExcel()
        {
            var financialAccounts = await _context.FinancialAccounts
                                                .Include(fa => fa.AccountType)
                                                .Include(u => u.User)
                                                .ToListAsync();

            if (!_currentUser.IsAdmin())
            {
                var userId = _currentUser.LoggedInUser();
                financialAccounts = financialAccounts.Where(t => t.UserId == userId).ToList();
            }

            var exportData = financialAccounts.Select(u => new
            {
                Id = u.Id,
                AccountType = u.AccountType!.Name,
                AccountBalance = $"{u.AccountBalance}$",
                Bank = u.Bank,
                CreatedDate = u.CreatedDate.ToString("dd-MM-yyyy"),
                User = $"{u.User!.FirstName} {u.User.LastName}"
            }).ToList();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("FinancialAccount");

            worksheet.Cell("A1").InsertTable(exportData);

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}
