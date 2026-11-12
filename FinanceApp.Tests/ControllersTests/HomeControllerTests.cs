using System.Security.Claims;
using FinanceApp.Controllers;
using FinanceApp.DTOs;
using FinanceApp.Helpers;
using FinanceApp.Models;
using FinanceApp.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinanceApp.Tests.ControllersTests
{
    public class HomeControllerTests
    {
        [Fact]
        public async Task Index_ReturnsAdminReportPopulatedWithReportServiceData()
        {
            var notifications = new Mock<INotificationService>();
            var reports = new Mock<IAdminReportsService>();
            var dailyTransactions = new List<DailyMonthlyTransactionDTO>
            {
                new() { Date = new DateTime(2026, 11, 12), TotalAmount = 125.50 }
            };
            var monthlyTransactions = new List<DailyMonthlyTransactionDTO>
            {
                new() { Date = new DateTime(2026, 11, 1), TotalAmount = 820.25 }
            };
            reports.Setup(x => x.CountCustomers()).ReturnsAsync(12);
            reports.Setup(x => x.CountUsers()).ReturnsAsync(15);
            reports.Setup(x => x.TotalTransactionsPerMonth()).ReturnsAsync(1250.75);
            reports.Setup(x => x.GetTransactionsLast7DaysAsync()).ReturnsAsync(dailyTransactions);
            reports.Setup(x => x.GetTransactionsLast7MonthsAsync()).ReturnsAsync(monthlyTransactions);
            reports.Setup(x => x.InActiveUsers()).ReturnsAsync(3);
            var controller = CreateController(notifications, reports);

            var result = await controller.Index();

            var view = Assert.IsType<ViewResult>(result);
            var report = Assert.IsType<AdminReportDTO>(view.Model);
            Assert.Equal(12, report.Customers);
            Assert.Equal(15, report.Users);
            Assert.Equal(1250.75, report.TransactionsPerMonth);
            Assert.Same(dailyTransactions, report.DailyTransactions);
            Assert.Same(monthlyTransactions, report.MonthlyTransactions);
            Assert.Equal(3, report.InActiveUsers);
            reports.Verify(x => x.CountCustomers(), Times.Once);
            reports.Verify(x => x.CountUsers(), Times.Once);
            reports.Verify(x => x.TotalTransactionsPerMonth(), Times.Once);
            reports.Verify(x => x.GetTransactionsLast7DaysAsync(), Times.Once);
            reports.Verify(x => x.GetTransactionsLast7MonthsAsync(), Times.Once);
            reports.Verify(x => x.InActiveUsers(), Times.Once);
        }

        [Fact]
        public async Task Logout_SignsOutAndRedirectsToAccountLogin()
        {
            var authentication = new Mock<IAuthenticationService>();
            var controller = CreateController(
                new Mock<INotificationService>(),
                new Mock<IAdminReportsService>(),
                authentication);

            var result = await controller.Logout();

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirect.ActionName);
            Assert.Equal("Account", redirect.ControllerName);
            authentication.Verify(x => x.SignOutAsync(
                controller.HttpContext,
                Settings.AuthCookieName,
                It.IsAny<AuthenticationProperties>()), Times.Once);
        }

        [Fact]
        public void Privacy_ReturnsView()
        {
            var controller = CreateController(new Mock<INotificationService>(), new Mock<IAdminReportsService>());

            var result = controller.Privacy();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Help_ReturnsView()
        {
            var controller = CreateController(new Mock<INotificationService>(), new Mock<IAdminReportsService>());

            var result = controller.Help();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Error_ReturnsErrorViewModelWithRequestId()
        {
            var controller = CreateController(new Mock<INotificationService>(), new Mock<IAdminReportsService>());
            controller.HttpContext.TraceIdentifier = "request-123";
            var expectedRequestId = System.Diagnostics.Activity.Current?.Id ?? "request-123";

            var result = controller.Error();

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ErrorViewModel>(view.Model);
            Assert.Equal(expectedRequestId, model.RequestId);
            Assert.True(model.ShowRequestId);
        }

        private static HomeController CreateController(
            Mock<INotificationService> notificationService,
            Mock<IAdminReportsService> reportsService,
            Mock<IAuthenticationService>? authenticationService = null)
        {
            var httpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    new[] { new Claim(ClaimTypes.NameIdentifier, "42") }, "test"))
            };
            httpContext.RequestServices = new ServiceCollection()
                .AddSingleton(authenticationService?.Object ?? Mock.Of<IAuthenticationService>())
                .BuildServiceProvider();

            return new HomeController(
                Mock.Of<ILogger<HomeController>>(),
                notificationService.Object,
                new CurrentUser(new HttpContextAccessor { HttpContext = httpContext }),
                reportsService.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = httpContext },
                TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
                    httpContext, Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>()),
                Url = Mock.Of<IUrlHelper>()
            };
        }
    }
}
