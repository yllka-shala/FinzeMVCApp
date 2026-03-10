using FinanceApp.DTOs;
using FinanceApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApp.Controllers
{
    public class CustomerReportsController : Controller
    {
        private readonly ICustomersReportService _service;

        public CustomerReportsController(ICustomersReportService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var totalTransactionsThisMonth = await _service.TotalTransactionsThisMonth();
            var getTransactionsLast7Days = await _service.GetTransactionsLast7DaysAsync();
            var getTransactionsLast7Months = await _service.GetTransactionsLast7MonthsAsync();
            var getTransactionsByCategoryChart = await _service.GetTransactionsByCategoryChart();
            var getMonthlySpentData = await _service.GetMonthlySpentData();
            var getBudgetsByCategoryData = await _service.GetBudgetsByCategoryData();
            var countCategories = await _service.CountCategories();
            var countFinancialAccounts = await _service.CountFinancialAccounts();
            var totalBudgetPerYear = await _service.TotalBudgetPerYear();

            var customerReport = new CustomerReportDTO
            {
                TotalTransactionsThisMonth = totalTransactionsThisMonth,
                GetTransactionsLast7Days = getTransactionsLast7Days,
                GetTransactionsLast7Months = getTransactionsLast7Months,
                GetTransactionsByCategoryChart = getTransactionsByCategoryChart,
                GetMonthlySpentData = getMonthlySpentData,
                GetBudgetsByCategoryData = getBudgetsByCategoryData,
                CountCategories = countCategories,
                CountFinancialAccounts = countFinancialAccounts,
                TotalBudgetPerYear = totalBudgetPerYear
            };

            return View(customerReport);
        }
    }
}
