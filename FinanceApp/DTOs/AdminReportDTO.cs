namespace FinanceApp.DTOs
{
    public class AdminReportDTO
    {
        public int? Customers {  get; set; }
        public int? Users { get; set; }
        public double? TransactionsPerMonth { get; set; }
        public List<DailyMonthlyTransactionDTO>? DailyTransactions { get; set; }
        public List<DailyMonthlyTransactionDTO>? MonthlyTransactions { get; set; }
        public int? InActiveUsers { get; set; }
    }
}
