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
    public class TransactionControllerTests
    {
        [Fact]
        public async Task Index_ReturnsTransactionsAndUsesRequestedPagingAndSearch()
        {
            var service = new Mock<ITransactionService>();
            var transactions = new PaginatedList<Transaction>(new List<Transaction> { CreateTransaction() }, 1, 2, 10);
            service.Setup(x => x.GetAll("rent", 2, 10)).ReturnsAsync(transactions);
            var controller = CreateController(service);

            var result = await controller.Index("rent", 2);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(transactions, view.Model);
            Assert.Equal("rent", controller.ViewData["CurrentFilter"]);
            service.Verify(x => x.GetAll("rent", 2, 10), Times.Once);
        }

        [Fact]
        public async Task Create_Get_ReturnsViewWithCategoriesAndPaymentMethods()
        {
            var service = new Mock<ITransactionService>();
            var categoryService = new Mock<ICategoryService>();
            var categories = new List<CategoryWithUserDto> { new() { Id = 3, Name = "Housing" } };
            var paymentMethods = new List<PaymentMethod> { new() { Id = 2, Name = "Card" } };
            categoryService.Setup(x => x.GetCategories()).ReturnsAsync(categories);
            service.Setup(x => x.GetAllPaymentMethods()).ReturnsAsync(paymentMethods);
            var controller = CreateController(service, categoryService);

            var result = await controller.Create();

            Assert.IsType<ViewResult>(result);
            Assert.Same(categories, controller.ViewBag.Categories);
            Assert.Same(paymentMethods, controller.ViewBag.PaymentMethods);
        }

        [Fact]
        public async Task Create_Post_WhenCreateSucceeds_RedirectsToIndex()
        {
            var service = new Mock<ITransactionService>();
            var request = CreateTransactionRequest();
            service.Setup(x => x.Create(request)).ReturnsAsync(CreateTransaction());
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
            var service = new Mock<ITransactionService>();
            var request = CreateTransactionRequest();
            service.Setup(x => x.Create(request)).ReturnsAsync((Transaction)null!);
            var controller = CreateController(service);

            var result = await controller.Create(request);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Error Occurred!", controller.TempData["error"]);
        }

        [Fact]
        public async Task Create_Post_WhenModelStateIsInvalid_ReturnsFormWithoutCreating()
        {
            var service = new Mock<ITransactionService>();
            var request = CreateTransactionRequest();
            var controller = CreateController(service);
            controller.ModelState.AddModelError("Description", "Required");

            var result = await controller.Create(request);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(request, view.Model);
            Assert.Equal("Error Occurred!", controller.TempData["error"]);
            service.Verify(x => x.Create(It.IsAny<TransactionDTO>()), Times.Never);
        }

        [Fact]
        public async Task Edit_Get_WhenIdIsNull_ReturnsNotFound()
        {
            var service = new Mock<ITransactionService>();
            var controller = CreateController(service);

            var result = await controller.Edit(null);

            Assert.IsType<NotFoundResult>(result);
            service.Verify(x => x.GetById(It.IsAny<int?>()), Times.Never);
        }

        [Fact]
        public async Task Edit_Get_WhenTransactionDoesNotExist_ReturnsNotFound()
        {
            var service = new Mock<ITransactionService>();
            service.Setup(x => x.GetById(404)).ReturnsAsync((Transaction)null!);
            var controller = CreateController(service);

            var result = await controller.Edit(404);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Get_WhenTransactionExists_ReturnsTransactionAndSelectOptions()
        {
            var service = new Mock<ITransactionService>();
            var categoryService = new Mock<ICategoryService>();
            var transaction = CreateTransaction();
            var categories = new List<CategoryWithUserDto> { new() { Id = 3, Name = "Housing" } };
            var paymentMethods = new List<PaymentMethod> { new() { Id = 2, Name = "Card" } };
            service.Setup(x => x.GetById(transaction.Id)).ReturnsAsync(transaction);
            categoryService.Setup(x => x.GetCategories()).ReturnsAsync(categories);
            service.Setup(x => x.GetAllPaymentMethods()).ReturnsAsync(paymentMethods);
            var controller = CreateController(service, categoryService);

            var result = await controller.Edit(transaction.Id);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(transaction, view.Model);
            Assert.Same(categories, controller.ViewBag.Categories);
            Assert.Same(paymentMethods, controller.ViewBag.PaymentMethods);
        }

        [Fact]
        public async Task Edit_Post_WhenIdIsNull_ReturnsNotFoundWithoutUpdating()
        {
            var service = new Mock<ITransactionService>();
            var controller = CreateController(service);

            var result = await controller.Edit(null, CreateTransactionRequest());

            Assert.IsType<NotFoundResult>(result);
            service.Verify(x => x.Update(It.IsAny<TransactionDTO>(), It.IsAny<int?>()), Times.Never);
        }

        [Fact]
        public async Task Edit_Post_WhenUpdateSucceeds_RedirectsToIndex()
        {
            var service = new Mock<ITransactionService>();
            var request = CreateTransactionRequest();
            service.Setup(x => x.Update(request, 5)).ReturnsAsync(CreateTransaction());
            var controller = CreateController(service);

            var result = await controller.Edit(5, request);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Data updated successfully!", controller.TempData["success"]);
        }

        [Fact]
        public async Task Edit_Post_WhenUpdateFails_RedirectsToIndexWithError()
        {
            var service = new Mock<ITransactionService>();
            var request = CreateTransactionRequest();
            service.Setup(x => x.Update(request, 5)).ReturnsAsync((Transaction)null!);
            var controller = CreateController(service);

            var result = await controller.Edit(5, request);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Error Occurred!", controller.TempData["error"]);
        }

        [Fact]
        public async Task Edit_Post_WhenModelStateIsInvalid_ReturnsFormWithoutUpdating()
        {
            var service = new Mock<ITransactionService>();
            var request = CreateTransactionRequest();
            var controller = CreateController(service);
            controller.ModelState.AddModelError("Description", "Required");

            var result = await controller.Edit(5, request);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(request, view.Model);
            Assert.Equal("Error Occurred!", controller.TempData["error"]);
            service.Verify(x => x.Update(It.IsAny<TransactionDTO>(), It.IsAny<int?>()), Times.Never);
        }

        [Fact]
        public async Task Delete_WhenIdIsNull_ReturnsNotFoundWithoutDeleting()
        {
            var service = new Mock<ITransactionService>();
            var controller = CreateController(service);

            var result = await controller.Delete(null);

            Assert.IsType<NotFoundResult>(result);
            service.Verify(x => x.Delete(It.IsAny<int?>()), Times.Never);
        }

        [Fact]
        public async Task Delete_WhenTransactionExists_DeletesAndRedirectsToIndex()
        {
            var service = new Mock<ITransactionService>();
            service.Setup(x => x.Delete(5)).ReturnsAsync(CreateTransaction());
            var controller = CreateController(service);

            var result = await controller.Delete(5);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Data deleted successfully!", controller.TempData["success"]);
        }

        [Fact]
        public async Task Delete_WhenTransactionDoesNotExist_RedirectsToIndexWithError()
        {
            var service = new Mock<ITransactionService>();
            service.Setup(x => x.Delete(404)).ReturnsAsync((Transaction)null!);
            var controller = CreateController(service);

            var result = await controller.Delete(404);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Error Occurred!", controller.TempData["error"]);
        }

        [Fact]
        public async Task ExportData_WhenFormatIsEmpty_RedirectsToIndexWithError()
        {
            var service = new Mock<ITransactionService>();
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
            var service = new Mock<ITransactionService>();
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
            var service = new Mock<ITransactionService>();
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

        private static TransactionController CreateController(
            Mock<ITransactionService> service,
            Mock<ICategoryService>? categoryService = null)
        {
            var httpContext = new DefaultHttpContext();
            return new TransactionController(service.Object, (categoryService ?? new Mock<ICategoryService>()).Object)
            {
                ControllerContext = new ControllerContext { HttpContext = httpContext },
                TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>())
            };
        }

        private static TransactionDTO CreateTransactionRequest() => new()
        {
            Description = "Monthly rent",
            Amount = 1200,
            CategoryId = 3,
            TransactionDate = new DateTime(2026, 11, 1),
            PaymentMethodId = 2,
            UserId = 42
        };

        private static Transaction CreateTransaction() => new()
        {
            Id = 5,
            Description = "Monthly rent",
            Amount = 1200,
            CategoryId = 3,
            TransactionDate = new DateTime(2026, 11, 1),
            PaymentMethodId = 2,
            UserId = 42
        };
    }
}
