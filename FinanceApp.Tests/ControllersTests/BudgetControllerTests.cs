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
    public class BudgetControllerTests
    {
        [Fact]
        public async Task Index_ReturnsBudgetsAndUsesRequestedPagingAndSearch()
        {
            var service = new Mock<IBudgetService>();
            var budgets = new PaginatedList<Budget>(new List<Budget> { CreateBudget() }, 1, 2, 10);
            service.Setup(x => x.GetAll("groceries", 2, 10)).ReturnsAsync(budgets);
            var controller = CreateController(service);

            var result = await controller.Index("groceries", 2);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(budgets, view.Model);
            Assert.Equal("groceries", controller.ViewData["CurrentFilter"]);
            service.Verify(x => x.GetAll("groceries", 2, 10), Times.Once);
        }

        [Fact]
        public async Task Create_Get_ReturnsViewWithCategories()
        {
            var service = new Mock<IBudgetService>();
            var categoryService = new Mock<ICategoryService>();
            var categories = new List<CategoryWithUserDto> { new() { Id = 3, Name = "Groceries" } };
            categoryService.Setup(x => x.GetCategories()).ReturnsAsync(categories);
            var controller = CreateController(service, categoryService);

            var result = await controller.Create();

            Assert.IsType<ViewResult>(result);
            Assert.Same(categories, controller.ViewBag.Category);
            categoryService.Verify(x => x.GetCategories(), Times.Once);
        }

        [Fact]
        public async Task Create_Post_WhenCreateSucceeds_RedirectsToIndex()
        {
            var service = new Mock<IBudgetService>();
            var request = CreateBudgetRequest();
            service.Setup(x => x.Create(request)).ReturnsAsync(CreateBudget());
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
            var service = new Mock<IBudgetService>();
            var request = CreateBudgetRequest();
            service.Setup(x => x.Create(request)).ReturnsAsync((Budget)null!);
            var controller = CreateController(service);

            var result = await controller.Create(request);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Error Occurred!", controller.TempData["error"]);
        }

        [Fact]
        public async Task Create_Post_WhenModelStateIsInvalid_ReturnsFormWithoutCreating()
        {
            var service = new Mock<IBudgetService>();
            var request = CreateBudgetRequest();
            var controller = CreateController(service);
            controller.ModelState.AddModelError("Limit", "Invalid limit");

            var result = await controller.Create(request);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(request, view.Model);
            Assert.Equal("Error Occurred!", controller.TempData["error"]);
            service.Verify(x => x.Create(It.IsAny<BudgetDTO>()), Times.Never);
        }

        [Fact]
        public async Task Edit_Get_WhenIdIsNull_ReturnsNotFound()
        {
            var service = new Mock<IBudgetService>();
            var controller = CreateController(service);

            var result = await controller.Edit(null, "food");

            Assert.IsType<NotFoundResult>(result);
            service.Verify(x => x.GetById(It.IsAny<int?>()), Times.Never);
        }

        [Fact]
        public async Task Edit_Get_WhenBudgetDoesNotExist_ReturnsNotFound()
        {
            var service = new Mock<IBudgetService>();
            var categoryService = new Mock<ICategoryService>();
            service.Setup(x => x.GetById(404)).ReturnsAsync((Budget)null!);
            var controller = CreateController(service, categoryService);

            var result = await controller.Edit(404, null);

            Assert.IsType<NotFoundResult>(result);
            categoryService.Verify(x => x.GetCategories(), Times.Never);
        }

        [Fact]
        public async Task Edit_Get_WhenBudgetExists_ReturnsBudgetAndCategories()
        {
            var service = new Mock<IBudgetService>();
            var categoryService = new Mock<ICategoryService>();
            var budget = CreateBudget();
            var categories = new List<CategoryWithUserDto> { new() { Id = 3, Name = "Groceries" } };
            service.Setup(x => x.GetById(budget.Id)).ReturnsAsync(budget);
            categoryService.Setup(x => x.GetCategories()).ReturnsAsync(categories);
            var controller = CreateController(service, categoryService);

            var result = await controller.Edit(budget.Id, "search-query");

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(budget, view.Model);
            Assert.Same(categories, controller.ViewBag.Category);
        }

        [Fact]
        public async Task Edit_Post_WhenIdIsNull_ReturnsNotFoundWithoutUpdating()
        {
            var service = new Mock<IBudgetService>();
            var controller = CreateController(service);

            var result = await controller.Edit(CreateBudgetRequest(), null);

            Assert.IsType<NotFoundResult>(result);
            service.Verify(x => x.Update(It.IsAny<BudgetDTO>(), It.IsAny<int?>()), Times.Never);
        }

        [Fact]
        public async Task Edit_Post_WhenUpdateSucceeds_RedirectsToIndex()
        {
            var service = new Mock<IBudgetService>();
            var request = CreateBudgetRequest();
            service.Setup(x => x.Update(request, 5)).ReturnsAsync(CreateBudget());
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
            var service = new Mock<IBudgetService>();
            var request = CreateBudgetRequest();
            service.Setup(x => x.Update(request, 5)).ReturnsAsync((Budget)null!);
            var controller = CreateController(service);

            var result = await controller.Edit(request, 5);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Error Occurred!", controller.TempData["error"]);
        }

        [Fact]
        public async Task Edit_Post_WhenModelStateIsInvalid_ReturnsFormWithoutUpdating()
        {
            var service = new Mock<IBudgetService>();
            var request = CreateBudgetRequest();
            var controller = CreateController(service);
            controller.ModelState.AddModelError("Limit", "Invalid limit");

            var result = await controller.Edit(request, 5);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(request, view.Model);
            Assert.Equal("Error Occurred!", controller.TempData["error"]);
            service.Verify(x => x.Update(It.IsAny<BudgetDTO>(), It.IsAny<int?>()), Times.Never);
        }

        [Fact]
        public async Task Delete_WhenIdIsNull_ReturnsNotFoundWithoutDeleting()
        {
            var service = new Mock<IBudgetService>();
            var controller = CreateController(service);

            var result = await controller.Delete(null);

            Assert.IsType<NotFoundResult>(result);
            service.Verify(x => x.Delete(It.IsAny<int?>()), Times.Never);
        }

        [Fact]
        public async Task Delete_WhenBudgetExists_DeletesAndRedirectsToIndex()
        {
            var service = new Mock<IBudgetService>();
            service.Setup(x => x.Delete(5)).ReturnsAsync(CreateBudget());
            var controller = CreateController(service);

            var result = await controller.Delete(5);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Data deleted successfully!", controller.TempData["success"]);
            service.Verify(x => x.Delete(5), Times.Once);
        }

        [Fact]
        public async Task Delete_WhenBudgetDoesNotExist_RedirectsToIndexWithError()
        {
            var service = new Mock<IBudgetService>();
            service.Setup(x => x.Delete(404)).ReturnsAsync((Budget)null!);
            var controller = CreateController(service);

            var result = await controller.Delete(404);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Error Occurred!", controller.TempData["error"]);
        }

        [Fact]
        public async Task ExportData_WhenFormatIsEmpty_RedirectsToIndexWithError()
        {
            var service = new Mock<IBudgetService>();
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
            var service = new Mock<IBudgetService>();
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
            var service = new Mock<IBudgetService>();
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

        private static BudgetController CreateController(
            Mock<IBudgetService> service,
            Mock<ICategoryService>? categoryService = null)
        {
            var httpContext = new DefaultHttpContext();
            return new BudgetController(service.Object, (categoryService ?? new Mock<ICategoryService>()).Object)
            {
                ControllerContext = new ControllerContext { HttpContext = httpContext },
                TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>())
            };
        }

        private static BudgetDTO CreateBudgetRequest() => new()
        {
            CategoryId = 3,
            Limit = 500,
            CurrentSpent = 125,
            Remaining = 375,
            UserId = 42
        };

        private static Budget CreateBudget() => new()
        {
            Id = 5,
            CategoryId = 3,
            Limit = 500,
            CurrentSpent = 125,
            Remaining = 375,
            UserId = 42
        };
    }
}
