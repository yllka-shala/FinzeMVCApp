using FinanceApp.Controllers;
using FinanceApp.DTOs;
using FinanceApp.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FinanceApp.Tests.ControllersTests
{
    public class CustomerReportsControllerTests
    {
        [Fact]
        public async Task Index_ReturnsReportPopulatedWithServiceData()
        {
            var service = new Mock<ICustomersReportService>();
            var last7Days = new List<DailyMonthlyTransactionDTO>
            {
                new() { Date = new DateTime(2026, 11, 12), TotalAmount = 125.50 }
            };
            var last7Months = new List<DailyMonthlyTransactionDTO>
            {
                new() { Date = new DateTime(2026, 11, 1), TotalAmount = 820.25 }
            };
            var transactionsByCategory = new List<CategoryChartDTO>
            {
                new() { Category = "Groceries", Total = 240.75 }
            };
            var monthlySpent = new List<MonthlySpentChartDTO>
            {
                new() { Label = "November", Total = 520.40 }
            };
            var budgetsByCategory = new List<CategoryChartDTO>
            {
                new() { Category = "Transport", Total = 150 }
            };

            service.Setup(x => x.TotalTransactionsThisMonth()).ReturnsAsync(1250.75);
            service.Setup(x => x.GetTransactionsLast7DaysAsync()).ReturnsAsync(last7Days);
            service.Setup(x => x.GetTransactionsLast7MonthsAsync()).ReturnsAsync(last7Months);
            service.Setup(x => x.GetTransactionsByCategoryChart()).ReturnsAsync(transactionsByCategory);
            service.Setup(x => x.GetMonthlySpentData()).ReturnsAsync(monthlySpent);
            service.Setup(x => x.GetBudgetsByCategoryData()).ReturnsAsync(budgetsByCategory);
            service.Setup(x => x.CountCategories()).ReturnsAsync(6);
            service.Setup(x => x.CountFinancialAccounts()).ReturnsAsync(3);
            service.Setup(x => x.TotalBudgetPerYear()).ReturnsAsync(24000.50);
            var controller = new CustomerReportsController(service.Object);

            var result = await controller.Index();

            var view = Assert.IsType<ViewResult>(result);
            var report = Assert.IsType<CustomerReportDTO>(view.Model);
            Assert.Equal(1250.75, report.TotalTransactionsThisMonth);
            Assert.Same(last7Days, report.GetTransactionsLast7Days);
            Assert.Same(last7Months, report.GetTransactionsLast7Months);
            Assert.Same(transactionsByCategory, report.GetTransactionsByCategoryChart);
            Assert.Same(monthlySpent, report.GetMonthlySpentData);
            Assert.Same(budgetsByCategory, report.GetBudgetsByCategoryData);
            Assert.Equal(6, report.CountCategories);
            Assert.Equal(3, report.CountFinancialAccounts);
            Assert.Equal(24000.50, report.TotalBudgetPerYear);

            service.Verify(x => x.TotalTransactionsThisMonth(), Times.Once);
            service.Verify(x => x.GetTransactionsLast7DaysAsync(), Times.Once);
            service.Verify(x => x.GetTransactionsLast7MonthsAsync(), Times.Once);
            service.Verify(x => x.GetTransactionsByCategoryChart(), Times.Once);
            service.Verify(x => x.GetMonthlySpentData(), Times.Once);
            service.Verify(x => x.GetBudgetsByCategoryData(), Times.Once);
            service.Verify(x => x.CountCategories(), Times.Once);
            service.Verify(x => x.CountFinancialAccounts(), Times.Once);
            service.Verify(x => x.TotalBudgetPerYear(), Times.Once);
        }
    }
}
