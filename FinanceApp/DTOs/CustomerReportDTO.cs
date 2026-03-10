using System.ComponentModel;

namespace FinanceApp.DTOs
{
    public class CustomerReportDTO
    {
        public double TotalTransactionsThisMonth { get; set; }
        public List<DailyMonthlyTransactionDTO>? GetTransactionsLast7Days { get; set; }
        public List<DailyMonthlyTransactionDTO>? GetTransactionsLast7Months { get; set; }
        public List<CategoryChartDTO>? GetTransactionsByCategoryChart { get; set; }
        public List<MonthlySpentChartDTO>? GetMonthlySpentData { get; set; }
        public List<CategoryChartDTO>? GetBudgetsByCategoryData { get; set; }
        public int CountCategories { get; set; }
        public int CountFinancialAccounts { get; set; }
        public double TotalBudgetPerYear { get; set; }
    }
}
