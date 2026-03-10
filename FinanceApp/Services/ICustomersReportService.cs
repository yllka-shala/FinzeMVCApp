using FinanceApp.DTOs;

namespace FinanceApp.Services
{
    public interface ICustomersReportService
    {
        Task<double> TotalTransactionsThisMonth();
        Task<int> CountFinancialAccounts();
        Task<int> CountCategories();
        Task<double> TotalBudgetPerYear();
        Task<List<DailyMonthlyTransactionDTO>> GetTransactionsLast7DaysAsync();
        Task<List<DailyMonthlyTransactionDTO>> GetTransactionsLast7MonthsAsync();
        Task<List<CategoryChartDTO>> GetTransactionsByCategoryChart();
        Task<List<MonthlySpentChartDTO>> GetMonthlySpentData();
        Task<List<CategoryChartDTO>> GetBudgetsByCategoryData();
    }
}
