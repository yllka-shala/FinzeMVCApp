using AspNetCoreGeneratedDocument;
using FinanceApp.Data;
using FinanceApp.DTOs;
using FinanceApp.Enums;
using FinanceApp.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Services
{
    public class AdminReportsService : IAdminReportsService
    {
        private readonly FinanceAppContext _context;
        private readonly CurrentUser _user;

        public AdminReportsService(FinanceAppContext context, CurrentUser user)
        {
            _context = context;
            _user = user;
        }

        public async Task<int> CountCustomers()
        {
            int data = await _context.Users
                                    .Where(u => u.Role.Name == RoleType.Customer.ToString() && 
                                                u.CreatedDate.Year == DateTime.Now.Year)
                                    .CountAsync();

            if (data == 0)
                return 0;

            return data;
        }

        public async Task<int> CountUsers()
        {
            int data = await _context.Users.CountAsync();

            if (data == 0)
                return 0;

            return data;
        }

        public async Task<double> TotalTransactionsPerMonth()
        {
            var now = DateTime.UtcNow;

            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var startOfNextMonth = startOfMonth.AddMonths(1);

            var monthlyTotal = await _context.Transactions
                .Where(t => t.TransactionDate >= startOfMonth &&
                            t.TransactionDate < startOfNextMonth)
                .SumAsync(t => t.Amount);

            if (monthlyTotal == 0)
                return 0;

            return monthlyTotal;
        }

        public async Task<List<DailyMonthlyTransactionDTO>> GetTransactionsLast7DaysAsync()
        {
            var startDate = DateTime.UtcNow.Date.AddDays(-6); 

            var data = await _context.Transactions
                .Where(t => t.TransactionDate >= startDate)
                .GroupBy(t => t.CreatedDate.Date)
                .Select(g => new 
                {
                    Date = g.Key,
                    TotalAmount = g.Sum(t => t.Amount)
                })
                .ToListAsync();

            var result = Enumerable.Range(0, 7)
                .Select(i => startDate.AddDays(i))
                .Select(date => new DailyMonthlyTransactionDTO
                {
                    Date = date,
                    TotalAmount = data.FirstOrDefault(d => d.Date == date)?.TotalAmount ?? 0
                })
                .ToList();

            return result;
        }

        public async Task<List<DailyMonthlyTransactionDTO>> GetTransactionsLast7MonthsAsync()
        {
            var startDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(-6);

            var data = await _context.Transactions
                .Where(t => t.TransactionDate >= startDate)
                .GroupBy(t => new { t.TransactionDate.Year, t.TransactionDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalAmount = g.Sum(t => t.Amount)
                })
                .ToListAsync();

            var result = Enumerable.Range(0, 7)
               .Select(i =>
               {
                   var date = startDate.AddMonths(i);
                   var record = data.FirstOrDefault(d => d.Year == date.Year && d.Month == date.Month);
                   return new DailyMonthlyTransactionDTO
                   {
                       Date = date,
                       TotalAmount = record?.TotalAmount ?? 0
                   };
               })
               .ToList();

            return result;
        }

        public async Task<int> InActiveUsers()
        {
            int data = await _context.Users.Where(u => u.IsActive == false).CountAsync();

            if (data == 0)
                return 0;

            return data;
        }
    }
}
