using FinanceApp.Data;
using FinanceApp.DTOs;
using FinanceApp.Enums;
using FinanceApp.Helpers;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Services
{
    public class CustomersReportService : ICustomersReportService
    {
        private readonly FinanceAppContext _context;
        private readonly CurrentUser _currentUser;

        public CustomersReportService(FinanceAppContext context, CurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }
        public async Task<List<DailyMonthlyTransactionDTO>> GetTransactionsLast7DaysAsync()
        {
            var startDate = DateTime.UtcNow.Date.AddDays(-6);

            var data = await _context.Transactions
                .Where(t => t.TransactionDate >= startDate && t.UserId == _currentUser.LoggedInUser())
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
                .Where(t => t.TransactionDate >= startDate && t.UserId == _currentUser.LoggedInUser())
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

        public async Task<double> TotalTransactionsThisMonth()
        {
            var now = DateTime.UtcNow;

            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var startOfNextMonth = startOfMonth.AddMonths(1);

            var monthlyTotal = await _context.Transactions
                .Where(t => t.TransactionDate >= startOfMonth &&
                            t.TransactionDate < startOfNextMonth &&
                            t.UserId == _currentUser.LoggedInUser())
                .SumAsync(t => t.Amount);

            if (monthlyTotal == 0)
                return 0;

            return monthlyTotal;
        }

        public async Task<List<CategoryChartDTO>> GetTransactionsByCategoryChart()
        {
            var data = await _context.Transactions.Where(t => t.UserId == _currentUser.LoggedInUser())
                                            .GroupBy(e => e.Category!.Name)
                                            .Select(g => new CategoryChartDTO
                                            {
                                                Category = g.Key,
                                                Total = g.Sum(e => e.Amount)
                                            })
                                            .AsNoTracking()
                                            .ToListAsync();

            if (data == null)
                return null;

            return data;
        }

        public async Task<List<MonthlySpentChartDTO>> GetMonthlySpentData()
        {
            var data = await _context.Transactions
                                       .Where(e => e.UserId == _currentUser.LoggedInUser())
                                       .GroupBy(e => new { e.TransactionDate.Year, e.TransactionDate.Month })
                                       .Select(g => new
                                       {
                                           Year = g.Key.Year,
                                           Month = g.Key.Month,
                                           Total = g.Sum(x => x.Amount)
                                       })
                                       .OrderBy(x => x.Year)
                                       .ThenBy(x => x.Month)
                                       .AsNoTracking()
                                       .ToListAsync();

            var result = data.Select(x => new MonthlySpentChartDTO
            {
                Label = new DateTime(x.Year, x.Month, 1).ToString("MMM yyyy"),
                Total = x.Total
            }).ToList();

            if (result == null)
                return null;

            return result;
        }

        public async Task<List<CategoryChartDTO>> GetBudgetsByCategoryData()
        {
            var data = await _context.Budgets.Where(t => t.UserId == _currentUser.LoggedInUser())
                                            .GroupBy(e => e.Category!.Name)
                                            .Select(g => new CategoryChartDTO
                                            {
                                                Category = g.Key,
                                                Total = g.Sum(e => e.CurrentSpent)
                                            })
                                            .AsNoTracking()
                                            .ToListAsync();

            if (data == null)
                return null;

            return data;
        }

        public async Task<int> CountFinancialAccounts()
        {
            int data = await _context.FinancialAccounts
                                    .Where(u => u.UserId == _currentUser.LoggedInUser())
                                    .CountAsync();

            if (data == 0)
                return 0;

            return data;
        }

        public async Task<int> CountCategories()
        {
            int data = await _context.Categories
                                    .Where(u => u.UserId == _currentUser.LoggedInUser())
                                    .CountAsync();

            if (data == 0)
                return 0;

            return data;
        }

        public async Task<double> TotalBudgetPerYear()
        {
            var yearlyTotal = await _context.Budgets
                .Where(t => t.CreatedDate.Year == DateTime.Now.Year &&
                            t.UserId == _currentUser.LoggedInUser())
                .SumAsync(t => t.Limit);

            if (yearlyTotal == 0)
                return 0;

            return yearlyTotal;
        }
    }
}
