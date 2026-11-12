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
    public class NotificationControllerTests
    {
        [Fact]
        public async Task Index_ReturnsNotificationsAndUsesRequestedPagingAndSearch()
        {
            var service = new Mock<INotificationService>();
            var notifications = new PaginatedList<Notification>(new List<Notification> { CreateNotification() }, 1, 2, 10);
            service.Setup(x => x.GetAll("budget", 2, 10)).ReturnsAsync(notifications);
            var controller = CreateController(service);

            var result = await controller.Index("budget", 2);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(notifications, view.Model);
            Assert.Equal("budget", controller.ViewData["CurrentFilter"]);
            service.Verify(x => x.GetAll("budget", 2, 10), Times.Once);
        }

        [Fact]
        public async Task Create_Get_ReturnsViewWithNotificationTypes()
        {
            var service = new Mock<INotificationService>();
            var notificationTypes = new List<NotificationType> { new() { Id = 1, Name = "Reminder" } };
            service.Setup(x => x.GetNotificationTypeList()).ReturnsAsync(notificationTypes);
            var controller = CreateController(service);

            var result = await controller.Create();

            Assert.IsType<ViewResult>(result);
            Assert.Same(notificationTypes, controller.ViewBag.NotificationType);
            service.Verify(x => x.GetNotificationTypeList(), Times.Once);
        }

        [Fact]
        public async Task Create_Post_WhenCreateSucceeds_RedirectsToIndex()
        {
            var service = new Mock<INotificationService>();
            var request = CreateNotificationRequest();
            service.Setup(x => x.Create(request)).ReturnsAsync(CreateNotification());
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
            var service = new Mock<INotificationService>();
            var request = CreateNotificationRequest();
            service.Setup(x => x.Create(request)).ReturnsAsync((Notification)null!);
            var controller = CreateController(service);

            var result = await controller.Create(request);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Error Occurred!", controller.TempData["error"]);
        }

        [Fact]
        public async Task Create_Post_WhenModelStateIsInvalid_ReturnsFormWithoutCreating()
        {
            var service = new Mock<INotificationService>();
            var request = CreateNotificationRequest();
            var controller = CreateController(service);
            controller.ModelState.AddModelError("Message", "Required");

            var result = await controller.Create(request);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(request, view.Model);
            Assert.Equal("Error Occurred!", controller.TempData["error"]);
            service.Verify(x => x.Create(It.IsAny<NotificationDTO>()), Times.Never);
        }

        [Fact]
        public async Task Edit_Get_WhenIdIsNull_ReturnsNotFound()
        {
            var service = new Mock<INotificationService>();
            var controller = CreateController(service);

            var result = await controller.Edit(null);

            Assert.IsType<NotFoundResult>(result);
            service.Verify(x => x.GetById(It.IsAny<int?>()), Times.Never);
        }

        [Fact]
        public async Task Edit_Get_WhenNotificationDoesNotExist_ReturnsNotFound()
        {
            var service = new Mock<INotificationService>();
            service.Setup(x => x.GetById(404)).ReturnsAsync((Notification)null!);
            var controller = CreateController(service);

            var result = await controller.Edit(404);

            Assert.IsType<NotFoundResult>(result);
            service.Verify(x => x.GetNotificationTypeList(), Times.Never);
        }

        [Fact]
        public async Task Edit_Get_WhenNotificationExists_ReturnsNotificationAndTypes()
        {
            var service = new Mock<INotificationService>();
            var notification = CreateNotification();
            var notificationTypes = new List<NotificationType> { new() { Id = 1, Name = "Reminder" } };
            service.Setup(x => x.GetById(notification.Id)).ReturnsAsync(notification);
            service.Setup(x => x.GetNotificationTypeList()).ReturnsAsync(notificationTypes);
            var controller = CreateController(service);

            var result = await controller.Edit(notification.Id);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(notification, view.Model);
            Assert.Same(notificationTypes, controller.ViewBag.NotificationType);
        }

        [Fact]
        public async Task Edit_Post_WhenIdIsNull_ReturnsNotFoundWithoutUpdating()
        {
            var service = new Mock<INotificationService>();
            var controller = CreateController(service);

            var result = await controller.Edit(CreateNotificationRequest(), null);

            Assert.IsType<NotFoundResult>(result);
            service.Verify(x => x.Update(It.IsAny<NotificationDTO>(), It.IsAny<int?>()), Times.Never);
        }

        [Fact]
        public async Task Edit_Post_WhenUpdateSucceeds_RedirectsToIndex()
        {
            var service = new Mock<INotificationService>();
            var request = CreateNotificationRequest();
            service.Setup(x => x.Update(request, 5)).ReturnsAsync(CreateNotification());
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
            var service = new Mock<INotificationService>();
            var request = CreateNotificationRequest();
            service.Setup(x => x.Update(request, 5)).ReturnsAsync((Notification)null!);
            var controller = CreateController(service);

            var result = await controller.Edit(request, 5);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Error Occurred!", controller.TempData["error"]);
        }

        [Fact]
        public async Task Edit_Post_WhenModelStateIsInvalid_ReturnsFormWithoutUpdating()
        {
            var service = new Mock<INotificationService>();
            var request = CreateNotificationRequest();
            var controller = CreateController(service);
            controller.ModelState.AddModelError("Message", "Required");

            var result = await controller.Edit(request, 5);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(request, view.Model);
            Assert.Equal("Error Occurred!", controller.TempData["error"]);
            service.Verify(x => x.Update(It.IsAny<NotificationDTO>(), It.IsAny<int?>()), Times.Never);
        }

        [Fact]
        public async Task Delete_WhenIdIsNull_ReturnsNotFoundWithoutDeleting()
        {
            var service = new Mock<INotificationService>();
            var controller = CreateController(service);

            var result = await controller.Delete(null);

            Assert.IsType<NotFoundResult>(result);
            service.Verify(x => x.Delete(It.IsAny<int?>()), Times.Never);
        }

        [Fact]
        public async Task Delete_WhenNotificationExists_DeletesAndRedirectsToIndex()
        {
            var service = new Mock<INotificationService>();
            service.Setup(x => x.Delete(5)).ReturnsAsync(CreateNotification());
            var controller = CreateController(service);

            var result = await controller.Delete(5);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Data deleted successfully!", controller.TempData["success"]);
            service.Verify(x => x.Delete(5), Times.Once);
        }

        [Fact]
        public async Task Delete_WhenNotificationDoesNotExist_RedirectsToIndexWithError()
        {
            var service = new Mock<INotificationService>();
            service.Setup(x => x.Delete(404)).ReturnsAsync((Notification)null!);
            var controller = CreateController(service);

            var result = await controller.Delete(404);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Error Occurred!", controller.TempData["error"]);
        }

        [Fact]
        public async Task ClearAll_ClearsNotificationsAndRedirectsToHomeIndex()
        {
            var service = new Mock<INotificationService>();
            service.Setup(x => x.ClearAll()).ReturnsAsync(new List<Notification> { CreateNotification() });
            var controller = CreateController(service);

            var result = await controller.ClearAll();

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Home", redirect.ControllerName);
            service.Verify(x => x.ClearAll(), Times.Once);
        }

        [Fact]
        public async Task ExportData_WhenFormatIsEmpty_RedirectsToIndexWithError()
        {
            var service = new Mock<INotificationService>();
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
            var service = new Mock<INotificationService>();
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
            var service = new Mock<INotificationService>();
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

        private static NotificationController CreateController(Mock<INotificationService> service)
        {
            var httpContext = new DefaultHttpContext();
            return new NotificationController(service.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = httpContext },
                TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>())
            };
        }

        private static NotificationDTO CreateNotificationRequest() => new()
        {
            NotificationTypeId = 1,
            Message = "Budget limit reached",
            Read = false,
            UserId = 42
        };

        private static Notification CreateNotification() => new()
        {
            Id = 5,
            NotificationTypeId = 1,
            Message = "Budget limit reached",
            Read = false,
            UserId = 42
        };
    }
}
