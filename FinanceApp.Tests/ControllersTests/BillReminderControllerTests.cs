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
    public class BillReminderControllerTests
    {
        [Fact]
        public async Task Index_ReturnsRemindersAndUsesRequestedPagingAndSearch()
        {
            var service = new Mock<IBillReminderService>();
            var reminders = new PaginatedList<BillReminder>(new List<BillReminder> { CreateReminder() }, 1, 2, 10);
            service.Setup(x => x.GetAll("electricity", 2, 10)).ReturnsAsync(reminders);
            var controller = CreateController(service);

            var result = await controller.Index("electricity", 2);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(reminders, view.Model);
            Assert.Equal("electricity", controller.ViewData["CurrentFilter"]);
            service.Verify(x => x.GetAll("electricity", 2, 10), Times.Once);
        }

        [Fact]
        public async Task Create_Get_ReturnsView()
        {
            var controller = CreateController(new Mock<IBillReminderService>());

            var result = await controller.Create();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Create_Post_WhenCreateSucceeds_RedirectsToIndex()
        {
            var service = new Mock<IBillReminderService>();
            var request = CreateReminderRequest();
            service.Setup(x => x.Create(request)).ReturnsAsync(CreateReminder());
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
            var service = new Mock<IBillReminderService>();
            var request = CreateReminderRequest();
            service.Setup(x => x.Create(request)).ReturnsAsync((BillReminder)null!);
            var controller = CreateController(service);

            var result = await controller.Create(request);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Error Occurred!", controller.TempData["error"]);
        }

        [Fact]
        public async Task Create_Post_WhenModelStateIsInvalid_ReturnsFormWithoutCreating()
        {
            var service = new Mock<IBillReminderService>();
            var request = CreateReminderRequest();
            var controller = CreateController(service);
            controller.ModelState.AddModelError("Name", "Required");

            var result = await controller.Create(request);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(request, view.Model);
            Assert.Equal("Error Occurred!", controller.TempData["error"]);
            service.Verify(x => x.Create(It.IsAny<BillReminderDTO>()), Times.Never);
        }

        [Fact]
        public async Task Edit_Get_WhenIdIsNull_ReturnsNotFound()
        {
            var service = new Mock<IBillReminderService>();
            var controller = CreateController(service);

            var result = await controller.Edit(null);

            Assert.IsType<NotFoundResult>(result);
            service.Verify(x => x.GetById(It.IsAny<int?>()), Times.Never);
        }

        [Fact]
        public async Task Edit_Get_WhenReminderDoesNotExist_ReturnsNotFound()
        {
            var service = new Mock<IBillReminderService>();
            service.Setup(x => x.GetById(404)).ReturnsAsync((BillReminder)null!);
            var controller = CreateController(service);

            var result = await controller.Edit(404);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Get_WhenReminderExists_ReturnsReminder()
        {
            var service = new Mock<IBillReminderService>();
            var reminder = CreateReminder();
            service.Setup(x => x.GetById(reminder.Id)).ReturnsAsync(reminder);
            var controller = CreateController(service);

            var result = await controller.Edit(reminder.Id);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(reminder, view.Model);
        }

        [Fact]
        public async Task Edit_Post_WhenIdIsNull_ReturnsNotFoundWithoutUpdating()
        {
            var service = new Mock<IBillReminderService>();
            var controller = CreateController(service);

            var result = await controller.Edit(CreateReminderRequest(), null);

            Assert.IsType<NotFoundResult>(result);
            service.Verify(x => x.Update(It.IsAny<BillReminderDTO>(), It.IsAny<int?>()), Times.Never);
        }

        [Fact]
        public async Task Edit_Post_WhenUpdateSucceeds_RedirectsToIndex()
        {
            var service = new Mock<IBillReminderService>();
            var request = CreateReminderRequest();
            service.Setup(x => x.Update(request, 5)).ReturnsAsync(CreateReminder());
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
            var service = new Mock<IBillReminderService>();
            var request = CreateReminderRequest();
            service.Setup(x => x.Update(request, 5)).ReturnsAsync((BillReminder)null!);
            var controller = CreateController(service);

            var result = await controller.Edit(request, 5);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Error Occurred!", controller.TempData["error"]);
        }

        [Fact]
        public async Task Edit_Post_WhenModelStateIsInvalid_ReturnsFormWithoutUpdating()
        {
            var service = new Mock<IBillReminderService>();
            var request = CreateReminderRequest();
            var controller = CreateController(service);
            controller.ModelState.AddModelError("Name", "Required");

            var result = await controller.Edit(request, 5);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(request, view.Model);
            Assert.Equal("Error Occurred!", controller.TempData["error"]);
            service.Verify(x => x.Update(It.IsAny<BillReminderDTO>(), It.IsAny<int?>()), Times.Never);
        }

        [Fact]
        public async Task Delete_WhenIdIsNull_ReturnsNotFoundWithoutDeleting()
        {
            var service = new Mock<IBillReminderService>();
            var controller = CreateController(service);

            var result = await controller.Delete(null);

            Assert.IsType<NotFoundResult>(result);
            service.Verify(x => x.Delete(It.IsAny<int?>()), Times.Never);
        }

        [Fact]
        public async Task Delete_WhenReminderExists_DeletesAndRedirectsToIndex()
        {
            var service = new Mock<IBillReminderService>();
            service.Setup(x => x.Delete(5)).ReturnsAsync(CreateReminder());
            var controller = CreateController(service);

            var result = await controller.Delete(5);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Data deleted successfully!", controller.TempData["success"]);
            service.Verify(x => x.Delete(5), Times.Once);
        }

        [Fact]
        public async Task Delete_WhenReminderDoesNotExist_RedirectsToIndexWithError()
        {
            var service = new Mock<IBillReminderService>();
            service.Setup(x => x.Delete(404)).ReturnsAsync((BillReminder)null!);
            var controller = CreateController(service);

            var result = await controller.Delete(404);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Error Occurred!", controller.TempData["error"]);
        }

        [Fact]
        public async Task ExportData_WhenFormatIsEmpty_RedirectsToIndexWithError()
        {
            var service = new Mock<IBillReminderService>();
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
            var service = new Mock<IBillReminderService>();
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
            var service = new Mock<IBillReminderService>();
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

        private static BillReminderController CreateController(Mock<IBillReminderService> service)
        {
            var httpContext = new DefaultHttpContext();
            return new BillReminderController(service.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = httpContext },
                TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>())
            };
        }

        private static BillReminderDTO CreateReminderRequest() => new()
        {
            Name = "Electricity bill",
            AmountDue = 120.50,
            DueDate = new DateTime(2026, 12, 15),
            IsPaid = false,
            UserId = 42
        };

        private static BillReminder CreateReminder() => new()
        {
            Id = 5,
            Name = "Electricity bill",
            AmountDue = 120.50,
            DueDate = new DateTime(2026, 12, 15),
            IsPaid = false,
            UserId = 42
        };
    }
}
