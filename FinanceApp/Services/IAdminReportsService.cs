using FinanceApp.DTOs;

namespace FinanceApp.Services
{
    public interface IAdminReportsService
    {
        Task<int> CountCustomers();
        Task<int> CountUsers();
        Task<double> TotalTransactionsPerMonth();
        Task<List<DailyMonthlyTransactionDTO>> GetTransactionsLast7DaysAsync();
        Task<List<DailyMonthlyTransactionDTO>> GetTransactionsLast7MonthsAsync();
        Task<int> InActiveUsers();
    }
}
