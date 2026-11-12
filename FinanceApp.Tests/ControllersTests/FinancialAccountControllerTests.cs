using FinanceApp.Controllers;
using FinanceApp.DTOs;
using FinanceApp.Helpers;
using FinanceApp.Models;
using FinanceApp.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace FinanceApp.Tests.ControllersTests
{
    public class FinancialAccountControllerTests
    {
        [Fact]
        public async Task Index_ReturnsAccountsAndUsesRequestedPagingAndSearch()
        {
            var service = new Mock<IFinancialAccountService>();
            var accounts = new PaginatedList<FinancialAccount>(new List<FinancialAccount> { CreateAccount() }, 1, 2, 10);
            service.Setup(x => x.GetAll("checking", 2, 10)).ReturnsAsync(accounts);
            var controller = CreateController(service);

            var result = await controller.Index("checking", 2);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(accounts, view.Model);
            Assert.Equal("checking", controller.ViewData["CurrentFilter"]);
            service.Verify(x => x.GetAll("checking", 2, 10), Times.Once);
        }

        [Fact]
        public async Task Create_Get_ReturnsViewWithAccountTypes()
        {
            var service = new Mock<IFinancialAccountService>();
            var accountTypes = new List<AccountType> { new() { Id = 1, Name = "Checking" } };
            service.Setup(x => x.GetAccountTypeList()).ReturnsAsync(accountTypes);
            var controller = CreateController(service);

            var result = await controller.Create();

            Assert.IsType<ViewResult>(result);
            Assert.Same(accountTypes, controller.ViewBag.AccountTypes);
            service.Verify(x => x.GetAccountTypeList(), Times.Once);
        }

        [Fact]
        public async Task Create_Post_WhenCreateSucceeds_RedirectsToIndex()
        {
            var service = new Mock<IFinancialAccountService>();
            var request = CreateAccountRequest();
            service.Setup(x => x.Create(request)).ReturnsAsync(CreateAccount());
            var controller = CreateController(service);

            var result = await controller.Create(request);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Data saved successfully!", controller.TempData["success"]);
            service.Verify(x => x.Create(request), Times.Once);
        }

        [Fact]
        public async Task Create_Post_WhenCreateFails_RedirectsToIndexWithError()
        {
            var service = new Mock<IFinancialAccountService>();
            var request = CreateAccountRequest();
            service.Setup(x => x.Create(request)).ReturnsAsync((FinancialAccount)null!);
            var controller = CreateController(service);

            var result = await controller.Create(request);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Error Occurred!", controller.TempData["error"]);
        }

        [Fact]
        public async Task Create_Post_WhenModelStateIsInvalid_ReturnsFormWithoutCreating()
        {
            var service = new Mock<IFinancialAccountService>();
            var request = CreateAccountRequest();
            var controller = CreateController(service);
            controller.ModelState.AddModelError("AccountBalance", "Invalid balance");

            var result = await controller.Create(request);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(request, view.Model);
            Assert.Equal("Error Occurred!", controller.TempData["error"]);
            service.Verify(x => x.Create(It.IsAny<FinancialAccountDTO>()), Times.Never);
        }

        [Fact]
        public async Task Edit_Get_WhenIdIsNull_ReturnsNotFound()
        {
            var service = new Mock<IFinancialAccountService>();
            var controller = CreateController(service);

            var result = await controller.Edit(null);

            Assert.IsType<NotFoundResult>(result);
            service.Verify(x => x.GetById(It.IsAny<int?>()), Times.Never);
        }

        [Fact]
        public async Task Edit_Get_WhenAccountDoesNotExist_ReturnsNotFound()
        {
            var service = new Mock<IFinancialAccountService>();
            service.Setup(x => x.GetById(404)).ReturnsAsync((FinancialAccount)null!);
            var controller = CreateController(service);

            var result = await controller.Edit(404);

            Assert.IsType<NotFoundResult>(result);
            service.Verify(x => x.GetAccountTypeList(), Times.Never);
        }

        [Fact]
        public async Task Edit_Get_WhenAccountExists_ReturnsAccountAndAccountTypes()
        {
            var service = new Mock<IFinancialAccountService>();
            var account = CreateAccount();
            var accountTypes = new List<AccountType> { new() { Id = 1, Name = "Checking" } };
            service.Setup(x => x.GetById(account.Id)).ReturnsAsync(account);
            service.Setup(x => x.GetAccountTypeList()).ReturnsAsync(accountTypes);
            var controller = CreateController(service);

            var result = await controller.Edit(account.Id);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(account, view.Model);
            Assert.Same(accountTypes, controller.ViewBag.AccountTypes);
        }

        [Fact]
        public async Task Edit_Post_WhenIdIsNull_ReturnsNotFoundWithoutUpdating()
        {
            var service = new Mock<IFinancialAccountService>();
            var controller = CreateController(service);

            var result = await controller.Edit(CreateAccountRequest(), null);

            Assert.IsType<NotFoundResult>(result);
            service.Verify(x => x.Update(It.IsAny<FinancialAccountDTO>(), It.IsAny<int?>()), Times.Never);
        }

        [Fact]
        public async Task Edit_Post_WhenUpdateSucceeds_RedirectsToIndex()
        {
            var service = new Mock<IFinancialAccountService>();
            var request = CreateAccountRequest();
            service.Setup(x => x.Update(request, 5)).ReturnsAsync(CreateAccount());
            var controller = CreateController(service);

            var result = await controller.Edit(request, 5);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Data updated successfully!", controller.TempData["success"]);
            service.Verify(x => x.Update(request, 5), Times.Once);
        }

        [Fact]
        public async Task Edit_Post_WhenUpdateFails_RedirectsToIndexWithError()
        {
            var service = new Mock<IFinancialAccountService>();
            var request = CreateAccountRequest();
            service.Setup(x => x.Update(request, 5)).ReturnsAsync((FinancialAccount)null!);
            var controller = CreateController(service);

            var result = await controller.Edit(request, 5);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Error Occurred!", controller.TempData["error"]);
        }

        [Fact]
        public async Task Edit_Post_WhenModelStateIsInvalid_ReturnsFormWithoutUpdating()
        {
            var service = new Mock<IFinancialAccountService>();
            var request = CreateAccountRequest();
            var controller = CreateController(service);
            controller.ModelState.AddModelError("AccountBalance", "Invalid balance");

            var result = await controller.Edit(request, 5);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(request, view.Model);
            Assert.Equal("Error Occurred!", controller.TempData["error"]);
            service.Verify(x => x.Update(It.IsAny<FinancialAccountDTO>(), It.IsAny<int?>()), Times.Never);
        }

        [Fact]
        public async Task Delete_WhenIdIsNull_ReturnsNotFoundWithoutDeleting()
        {
            var service = new Mock<IFinancialAccountService>();
            var controller = CreateController(service);

            var result = await controller.Delete(null);

            Assert.IsType<NotFoundResult>(result);
            service.Verify(x => x.Delete(It.IsAny<int?>()), Times.Never);
        }

        [Fact]
        public async Task Delete_WhenAccountExists_DeletesAndRedirectsToIndex()
        {
            var service = new Mock<IFinancialAccountService>();
            service.Setup(x => x.Delete(5)).ReturnsAsync(CreateAccount());
            var controller = CreateController(service);

            var result = await controller.Delete(5);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Data deleted successfully!", controller.TempData["success"]);
            service.Verify(x => x.Delete(5), Times.Once);
        }

        [Fact]
        public async Task Delete_WhenAccountDoesNotExist_RedirectsToIndexWithError()
        {
            var service = new Mock<IFinancialAccountService>();
            service.Setup(x => x.Delete(404)).ReturnsAsync((FinancialAccount)null!);
            var controller = CreateController(service);

            var result = await controller.Delete(404);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Error Occurred!", controller.TempData["error"]);
        }

        [Fact]
        public async Task ExportData_WhenFormatIsEmpty_RedirectsToIndexWithError()
        {
            var service = new Mock<IFinancialAccountService>();
            var controller = CreateController(service);

            var result = await controller.ExportData(string.Empty);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Invalid format!", controller.TempData["error"]);
            service.Verify(x => x.ExportExcel(), Times.Never);
            service.Verify(x => x.ExportPdf(), Times.Never);
        }

        [Fact]
        public async Task ExportData_WhenFormatIsExcel_ReturnsExcelFile()
        {
            var service = new Mock<IFinancialAccountService>();
            var content = new byte[] { 1, 2, 3 };
            service.Setup(x => x.ExportExcel()).ReturnsAsync(content);
            var controller = CreateController(service);

            var result = await controller.ExportData("EXCEL");

            var file = Assert.IsType<FileContentResult>(result);
            Assert.Same(content, file.FileContents);
            Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", file.ContentType);
            Assert.EndsWith(".xlsx", file.FileDownloadName);
            service.Verify(x => x.ExportExcel(), Times.Once);
        }

        [Fact]
        public async Task ExportData_WhenFormatIsNotExcel_ReturnsPdfFile()
        {
            var service = new Mock<IFinancialAccountService>();
            var content = new byte[] { 4, 5, 6 };
            service.Setup(x => x.ExportPdf()).ReturnsAsync(content);
            var controller = CreateController(service);

            var result = await controller.ExportData("pdf");

            var file = Assert.IsType<FileContentResult>(result);
            Assert.Same(content, file.FileContents);
            Assert.Equal("application/pdf", file.ContentType);
            Assert.EndsWith(".pdf", file.FileDownloadName);
            service.Verify(x => x.ExportPdf(), Times.Once);
        }

        private static FinancialAccountController CreateController(Mock<IFinancialAccountService> service)
        {
            var httpContext = new DefaultHttpContext();
            return new FinancialAccountController(service.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = httpContext },
                TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>())
            };
        }

        private static FinancialAccountDTO CreateAccountRequest() => new()
        {
            AccountTypeId = 1,
            AccountBalance = 5250.75,
            Bank = "Northstar Bank",
            UserId = 42
        };

        private static FinancialAccount CreateAccount() => new()
        {
            Id = 5,
            AccountTypeId = 1,
            AccountBalance = 5250.75,
            Bank = "Northstar Bank",
            UserId = 42
        };
    }
}
