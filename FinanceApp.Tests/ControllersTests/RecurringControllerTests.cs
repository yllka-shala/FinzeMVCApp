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
    public class RecurringControllerTests
    {
        [Fact]
        public async Task Index_ReturnsRecurringItemsAndUsesRequestedPagingAndSearch()
        {
            var service = new Mock<IRecurringService>();
            var recurringItems = new PaginatedList<Recurring>(new List<Recurring> { CreateRecurring() }, 1, 2, 10);
            service.Setup(x => x.GetAll("rent", 2, 10)).ReturnsAsync(recurringItems);
            var controller = CreateController(service);

            var result = await controller.Index("rent", 2);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(recurringItems, view.Model);
            Assert.Equal("rent", controller.ViewData["CurrentFilter"]);
            service.Verify(x => x.GetAll("rent", 2, 10), Times.Once);
        }

        [Fact]
        public async Task Create_Get_ReturnsViewWithFrequencyOptions()
        {
            var service = new Mock<IRecurringService>();
            var frequencies = new List<Frequency> { new() { Id = 1, Name = "Monthly" } };
            service.Setup(x => x.GetFrequenciesList()).ReturnsAsync(frequencies);
            var controller = CreateController(service);

            var result = await controller.Create();

            Assert.IsType<ViewResult>(result);
            Assert.Same(frequencies, controller.ViewBag.Frequencies);
            service.Verify(x => x.GetFrequenciesList(), Times.Once);
        }

        [Fact]
        public async Task Create_Post_WhenCreateSucceeds_RedirectsToIndex()
        {
            var service = new Mock<IRecurringService>();
            var request = CreateRecurringRequest();
            service.Setup(x => x.Create(request)).ReturnsAsync(CreateRecurring());
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
            var service = new Mock<IRecurringService>();
            var request = CreateRecurringRequest();
            service.Setup(x => x.Create(request)).ReturnsAsync((Recurring)null!);
            var controller = CreateController(service);

            var result = await controller.Create(request);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Error Occurred!", controller.TempData["error"]);
        }

        [Fact]
        public async Task Create_Post_WhenModelStateIsInvalid_ReturnsFormWithoutCreating()
        {
            var service = new Mock<IRecurringService>();
            var request = CreateRecurringRequest();
            var controller = CreateController(service);
            controller.ModelState.AddModelError("Name", "Required");

            var result = await controller.Create(request);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(request, view.Model);
            Assert.Equal("Error Occurred!", controller.TempData["error"]);
            service.Verify(x => x.Create(It.IsAny<RecurringDTO>()), Times.Never);
        }

        [Fact]
        public async Task Edit_Get_WhenIdIsNull_ReturnsNotFound()
        {
            var service = new Mock<IRecurringService>();
            var controller = CreateController(service);

            var result = await controller.Edit(null);

            Assert.IsType<NotFoundResult>(result);
            service.Verify(x => x.GetById(It.IsAny<int?>()), Times.Never);
        }

        [Fact]
        public async Task Edit_Get_WhenRecurringItemDoesNotExist_ReturnsNotFound()
        {
            var service = new Mock<IRecurringService>();
            service.Setup(x => x.GetById(404)).ReturnsAsync((Recurring)null!);
            var controller = CreateController(service);

            var result = await controller.Edit(404);

            Assert.IsType<NotFoundResult>(result);
            service.Verify(x => x.GetFrequenciesList(), Times.Never);
        }

        [Fact]
        public async Task Edit_Get_WhenRecurringItemExists_ReturnsItemAndFrequencyOptions()
        {
            var service = new Mock<IRecurringService>();
            var recurring = CreateRecurring();
            var frequencies = new List<Frequency> { new() { Id = 1, Name = "Monthly" } };
            service.Setup(x => x.GetById(recurring.Id)).ReturnsAsync(recurring);
            service.Setup(x => x.GetFrequenciesList()).ReturnsAsync(frequencies);
            var controller = CreateController(service);

            var result = await controller.Edit(recurring.Id);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(recurring, view.Model);
            Assert.Same(frequencies, controller.ViewBag.Frequencies);
        }

        [Fact]
        public async Task Edit_Post_WhenIdIsNull_ReturnsNotFoundWithoutUpdating()
        {
            var service = new Mock<IRecurringService>();
            var controller = CreateController(service);

            var result = await controller.Edit(CreateRecurringRequest(), null);

            Assert.IsType<NotFoundResult>(result);
            service.Verify(x => x.Update(It.IsAny<RecurringDTO>(), It.IsAny<int?>()), Times.Never);
        }

        [Fact]
        public async Task Edit_Post_WhenUpdateSucceeds_RedirectsToIndex()
        {
            var service = new Mock<IRecurringService>();
            var request = CreateRecurringRequest();
            service.Setup(x => x.Update(request, 5)).ReturnsAsync(CreateRecurring());
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
            var service = new Mock<IRecurringService>();
            var request = CreateRecurringRequest();
            service.Setup(x => x.Update(request, 5)).ReturnsAsync((Recurring)null!);
            var controller = CreateController(service);

            var result = await controller.Edit(request, 5);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Error Occurred!", controller.TempData["error"]);
        }

        [Fact]
        public async Task Edit_Post_WhenModelStateIsInvalid_ReturnsFormWithoutUpdating()
        {
            var service = new Mock<IRecurringService>();
            var request = CreateRecurringRequest();
            var controller = CreateController(service);
            controller.ModelState.AddModelError("Name", "Required");

            var result = await controller.Edit(request, 5);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(request, view.Model);
            Assert.Equal("Error Occurred!", controller.TempData["error"]);
            service.Verify(x => x.Update(It.IsAny<RecurringDTO>(), It.IsAny<int?>()), Times.Never);
        }

        [Fact]
        public async Task Delete_WhenIdIsNull_ReturnsNotFoundWithoutDeleting()
        {
            var service = new Mock<IRecurringService>();
            var controller = CreateController(service);

            var result = await controller.Delete(null);

            Assert.IsType<NotFoundResult>(result);
            service.Verify(x => x.Delete(It.IsAny<int?>()), Times.Never);
        }

        [Fact]
        public async Task Delete_WhenRecurringItemExists_DeletesAndRedirectsToIndex()
        {
            var service = new Mock<IRecurringService>();
            service.Setup(x => x.Delete(5)).ReturnsAsync(CreateRecurring());
            var controller = CreateController(service);

            var result = await controller.Delete(5);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Data deleted successfully!", controller.TempData["success"]);
            service.Verify(x => x.Delete(5), Times.Once);
        }

        [Fact]
        public async Task Delete_WhenRecurringItemDoesNotExist_RedirectsToIndexWithError()
        {
            var service = new Mock<IRecurringService>();
            service.Setup(x => x.Delete(404)).ReturnsAsync((Recurring)null!);
            var controller = CreateController(service);

            var result = await controller.Delete(404);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Error Occurred!", controller.TempData["error"]);
        }

        [Fact]
        public async Task ExportData_WhenFormatIsEmpty_RedirectsToIndexWithError()
        {
            var service = new Mock<IRecurringService>();
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
            var service = new Mock<IRecurringService>();
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
            var service = new Mock<IRecurringService>();
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

        private static RecurringController CreateController(Mock<IRecurringService> service)
        {
            var httpContext = new DefaultHttpContext();
            return new RecurringController(service.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = httpContext },
                TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>())
            };
        }

        private static RecurringDTO CreateRecurringRequest() => new()
        {
            Name = "Monthly rent",
            Amount = 1200,
            DueDate = new DateTime(2026, 12, 1),
            FrequencyId = 1,
            UserId = 42
        };

        private static Recurring CreateRecurring() => new()
        {
            Id = 5,
            Name = "Monthly rent",
            Amount = 1200,
            DueDate = new DateTime(2026, 12, 1),
            FrequencyId = 1,
            UserId = 42
        };
    }
}
