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
    public class CategoryControllerTests
    {
        [Fact]
        public async Task Index_ReturnsCategoriesAndUsesRequestedPagingAndSearch()
        {
            var service = new Mock<ICategoryService>();
            var categories = new PaginatedList<CategoryWithUserDto>(
                new List<CategoryWithUserDto> { new() { Id = 3, Name = "Groceries", UserId = 42 } }, 1, 2, 10);
            service.Setup(x => x.GetAll("groceries", 2, 10)).ReturnsAsync(categories);
            var controller = CreateController(service);

            var result = await controller.Index("groceries", 2);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(categories, view.Model);
            Assert.Equal("groceries", controller.ViewData["CurrentFilter"]);
            service.Verify(x => x.GetAll("groceries", 2, 10), Times.Once);
        }

        [Fact]
        public async Task Create_Get_ReturnsView()
        {
            var controller = CreateController(new Mock<ICategoryService>());

            var result = await controller.Create();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Create_Post_WhenCreateSucceeds_RedirectsToIndex()
        {
            var service = new Mock<ICategoryService>();
            var request = CreateCategoryRequest();
            service.Setup(x => x.Create(request)).ReturnsAsync(CreateCategory());
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
            var service = new Mock<ICategoryService>();
            var request = CreateCategoryRequest();
            service.Setup(x => x.Create(request)).ReturnsAsync((Category)null!);
            var controller = CreateController(service);

            var result = await controller.Create(request);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Error Occurred!", controller.TempData["error"]);
        }

        [Fact]
        public async Task Create_Post_WhenModelStateIsInvalid_ReturnsFormWithoutCreating()
        {
            var service = new Mock<ICategoryService>();
            var request = CreateCategoryRequest();
            var controller = CreateController(service);
            controller.ModelState.AddModelError("Name", "Required");

            var result = await controller.Create(request);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(request, view.Model);
            Assert.Equal("Error Occurred!", controller.TempData["error"]);
            service.Verify(x => x.Create(It.IsAny<CategoryDTO>()), Times.Never);
        }

        [Fact]
        public async Task Edit_Get_WhenIdIsNull_ReturnsNotFound()
        {
            var service = new Mock<ICategoryService>();
            var controller = CreateController(service);

            var result = await controller.Edit(null);

            Assert.IsType<NotFoundResult>(result);
            service.Verify(x => x.GetById(It.IsAny<int?>()), Times.Never);
        }

        [Fact]
        public async Task Edit_Get_WhenCategoryDoesNotExist_ReturnsNotFound()
        {
            var service = new Mock<ICategoryService>();
            service.Setup(x => x.GetById(404)).ReturnsAsync((Category)null!);
            var controller = CreateController(service);

            var result = await controller.Edit(404);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Get_WhenCategoryExists_ReturnsCategory()
        {
            var service = new Mock<ICategoryService>();
            var category = CreateCategory();
            service.Setup(x => x.GetById(category.Id)).ReturnsAsync(category);
            var controller = CreateController(service);

            var result = await controller.Edit(category.Id);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(category, view.Model);
        }

        [Fact]
        public async Task Edit_Post_WhenIdIsNull_ReturnsNotFoundWithoutUpdating()
        {
            var service = new Mock<ICategoryService>();
            var controller = CreateController(service);

            var result = await controller.Edit(null, CreateCategoryRequest());

            Assert.IsType<NotFoundResult>(result);
            service.Verify(x => x.Update(It.IsAny<CategoryDTO>(), It.IsAny<int?>()), Times.Never);
        }

        [Fact]
        public async Task Edit_Post_WhenUpdateSucceeds_RedirectsToIndex()
        {
            var service = new Mock<ICategoryService>();
            var request = CreateCategoryRequest();
            service.Setup(x => x.Update(request, 5)).ReturnsAsync(CreateCategory());
            var controller = CreateController(service);

            var result = await controller.Edit(5, request);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Data updated successfully!", controller.TempData["success"]);
            service.Verify(x => x.Update(request, 5), Times.Once);
        }

        [Fact]
        public async Task Edit_Post_WhenUpdateFails_RedirectsToIndexWithError()
        {
            var service = new Mock<ICategoryService>();
            var request = CreateCategoryRequest();
            service.Setup(x => x.Update(request, 5)).ReturnsAsync((Category)null!);
            var controller = CreateController(service);

            var result = await controller.Edit(5, request);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Error Occurred!", controller.TempData["error"]);
        }

        [Fact]
        public async Task Edit_Post_WhenModelStateIsInvalid_ReturnsFormWithoutUpdating()
        {
            var service = new Mock<ICategoryService>();
            var request = CreateCategoryRequest();
            var controller = CreateController(service);
            controller.ModelState.AddModelError("Name", "Required");

            var result = await controller.Edit(5, request);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(request, view.Model);
            Assert.Equal("Error Occurred!", controller.TempData["error"]);
            service.Verify(x => x.Update(It.IsAny<CategoryDTO>(), It.IsAny<int?>()), Times.Never);
        }

        [Fact]
        public async Task Delete_WhenIdIsNull_ReturnsNotFoundWithoutDeleting()
        {
            var service = new Mock<ICategoryService>();
            var controller = CreateController(service);

            var result = await controller.Delete(null);

            Assert.IsType<NotFoundResult>(result);
            service.Verify(x => x.Delete(It.IsAny<int?>()), Times.Never);
        }

        [Fact]
        public async Task Delete_WhenCategoryExists_DeletesAndRedirectsToIndex()
        {
            var service = new Mock<ICategoryService>();
            service.Setup(x => x.Delete(5)).ReturnsAsync(CreateCategory());
            var controller = CreateController(service);

            var result = await controller.Delete(5);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Data deleted successfully!", controller.TempData["success"]);
            service.Verify(x => x.Delete(5), Times.Once);
        }

        [Fact]
        public async Task Delete_WhenCategoryDoesNotExist_RedirectsToIndexWithError()
        {
            var service = new Mock<ICategoryService>();
            service.Setup(x => x.Delete(404)).ReturnsAsync((Category)null!);
            var controller = CreateController(service);

            var result = await controller.Delete(404);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Error Occurred!", controller.TempData["error"]);
        }

        [Fact]
        public async Task ExportData_WhenFormatIsEmpty_RedirectsToIndexWithError()
        {
            var service = new Mock<ICategoryService>();
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
            var service = new Mock<ICategoryService>();
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
            var service = new Mock<ICategoryService>();
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

        private static CategoryController CreateController(Mock<ICategoryService> service)
        {
            var httpContext = new DefaultHttpContext();
            return new CategoryController(service.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = httpContext },
                TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>())
            };
        }

        private static CategoryDTO CreateCategoryRequest() => new()
        {
            Name = "Groceries",
            UserId = 42
        };

        private static Category CreateCategory() => new()
        {
            Id = 5,
            Name = "Groceries",
            UserId = 42
        };
    }
}
