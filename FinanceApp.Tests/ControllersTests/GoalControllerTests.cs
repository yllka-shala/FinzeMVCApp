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
    public class GoalControllerTests
    {
        [Fact]
        public async Task Index_ReturnsGoalsAndUsesRequestedPagingAndSearch()
        {
            var service = new Mock<IGoalService>();
            var goals = new PaginatedList<Goal>(new List<Goal> { CreateGoal() }, 1, 2, 10);
            service.Setup(x => x.GetAll("vacation", 2, 10)).ReturnsAsync(goals);
            var controller = CreateController(service);

            var result = await controller.Index("vacation", 2);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(goals, view.Model);
            Assert.Equal("vacation", controller.ViewData["CurrentFilter"]);
            service.Verify(x => x.GetAll("vacation", 2, 10), Times.Once);
        }

        [Fact]
        public async Task Create_Get_ReturnsView()
        {
            var controller = CreateController(new Mock<IGoalService>());

            var result = await controller.Create();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Create_Post_WhenCreateSucceeds_RedirectsToIndex()
        {
            var service = new Mock<IGoalService>();
            var request = CreateGoalRequest();
            service.Setup(x => x.Create(request)).ReturnsAsync(CreateGoal());
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
            var service = new Mock<IGoalService>();
            var request = CreateGoalRequest();
            service.Setup(x => x.Create(request)).ReturnsAsync((Goal)null!);
            var controller = CreateController(service);

            var result = await controller.Create(request);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Error Occurred!", controller.TempData["error"]);
        }

        [Fact]
        public async Task Create_Post_WhenModelStateIsInvalid_ReturnsFormWithoutCreating()
        {
            var service = new Mock<IGoalService>();
            var request = CreateGoalRequest();
            var controller = CreateController(service);
            controller.ModelState.AddModelError("Name", "Required");

            var result = await controller.Create(request);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(request, view.Model);
            Assert.Equal("Error Occurred!", controller.TempData["error"]);
            service.Verify(x => x.Create(It.IsAny<GoalDTO>()), Times.Never);
        }

        [Fact]
        public async Task Edit_Get_WhenIdIsNull_ReturnsNotFound()
        {
            var service = new Mock<IGoalService>();
            var controller = CreateController(service);

            var result = await controller.Edit(null);

            Assert.IsType<NotFoundResult>(result);
            service.Verify(x => x.GetById(It.IsAny<int?>()), Times.Never);
        }

        [Fact]
        public async Task Edit_Get_WhenGoalDoesNotExist_ReturnsNotFound()
        {
            var service = new Mock<IGoalService>();
            service.Setup(x => x.GetById(404)).ReturnsAsync((Goal)null!);
            var controller = CreateController(service);

            var result = await controller.Edit(404);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Get_WhenGoalExists_ReturnsGoal()
        {
            var service = new Mock<IGoalService>();
            var goal = CreateGoal();
            service.Setup(x => x.GetById(goal.Id)).ReturnsAsync(goal);
            var controller = CreateController(service);

            var result = await controller.Edit(goal.Id);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(goal, view.Model);
        }

        [Fact]
        public async Task Edit_Post_WhenIdIsNull_ReturnsNotFoundWithoutUpdating()
        {
            var service = new Mock<IGoalService>();
            var controller = CreateController(service);

            var result = await controller.Edit(CreateGoalRequest(), null);

            Assert.IsType<NotFoundResult>(result);
            service.Verify(x => x.Update(It.IsAny<GoalDTO>(), It.IsAny<int?>()), Times.Never);
        }

        [Fact]
        public async Task Edit_Post_WhenUpdateSucceeds_RedirectsToIndex()
        {
            var service = new Mock<IGoalService>();
            var request = CreateGoalRequest();
            service.Setup(x => x.Update(request, 5)).ReturnsAsync(CreateGoal());
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
            var service = new Mock<IGoalService>();
            var request = CreateGoalRequest();
            service.Setup(x => x.Update(request, 5)).ReturnsAsync((Goal)null!);
            var controller = CreateController(service);

            var result = await controller.Edit(request, 5);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Error Occurred!", controller.TempData["error"]);
        }

        [Fact]
        public async Task Edit_Post_WhenModelStateIsInvalid_ReturnsFormWithoutUpdating()
        {
            var service = new Mock<IGoalService>();
            var request = CreateGoalRequest();
            var controller = CreateController(service);
            controller.ModelState.AddModelError("Name", "Required");

            var result = await controller.Edit(request, 5);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(request, view.Model);
            Assert.Equal("Error Occurred!", controller.TempData["error"]);
            service.Verify(x => x.Update(It.IsAny<GoalDTO>(), It.IsAny<int?>()), Times.Never);
        }

        [Fact]
        public async Task Delete_WhenIdIsNull_ReturnsNotFoundWithoutDeleting()
        {
            var service = new Mock<IGoalService>();
            var controller = CreateController(service);

            var result = await controller.Delete(null);

            Assert.IsType<NotFoundResult>(result);
            service.Verify(x => x.Delete(It.IsAny<int?>()), Times.Never);
        }

        [Fact]
        public async Task Delete_WhenGoalExists_DeletesAndRedirectsToIndex()
        {
            var service = new Mock<IGoalService>();
            service.Setup(x => x.Delete(5)).ReturnsAsync(CreateGoal());
            var controller = CreateController(service);

            var result = await controller.Delete(5);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Data deleted successfully!", controller.TempData["success"]);
            service.Verify(x => x.Delete(5), Times.Once);
        }

        [Fact]
        public async Task Delete_WhenGoalDoesNotExist_RedirectsToIndexWithError()
        {
            var service = new Mock<IGoalService>();
            service.Setup(x => x.Delete(404)).ReturnsAsync((Goal)null!);
            var controller = CreateController(service);

            var result = await controller.Delete(404);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Error Occurred!", controller.TempData["error"]);
        }

        [Fact]
        public async Task ExportData_WhenFormatIsEmpty_RedirectsToIndexWithError()
        {
            var service = new Mock<IGoalService>();
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
            var service = new Mock<IGoalService>();
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
            var service = new Mock<IGoalService>();
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

        private static GoalController CreateController(Mock<IGoalService> service)
        {
            var httpContext = new DefaultHttpContext();
            return new GoalController(service.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = httpContext },
                TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>())
            };
        }

        private static GoalDTO CreateGoalRequest() => new()
        {
            Name = "Emergency fund",
            TargetAmount = 10000,
            CurrentAmount = 2500,
            DueDate = new DateTime(2027, 12, 31),
            UserId = 42
        };

        private static Goal CreateGoal() => new()
        {
            Id = 5,
            Name = "Emergency fund",
            TargetAmount = 10000,
            CurrentAmount = 2500,
            DueDate = new DateTime(2027, 12, 31),
            UserId = 42
        };
    }
}
