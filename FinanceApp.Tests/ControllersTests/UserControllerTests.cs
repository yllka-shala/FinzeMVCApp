using System.Security.Claims;
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
    public class UserControllerTests
    {
        [Fact]
        public async Task Index_ReturnsUsersAndUsesRequestedPagingAndSearch()
        {
            var service = new Mock<IUserService>();
            var users = new PaginatedList<User>(new List<User> { CreateUser() }, 1, 3, 10);
            service.Setup(x => x.GetAll("Morgan", 3, 10)).ReturnsAsync(users);
            var controller = CreateController(service);

            var result = await controller.Index("Morgan", 3);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(users, view.Model);
            Assert.Equal("Morgan", controller.ViewData["CurrentFilter"]);
            service.Verify(x => x.GetAll("Morgan", 3, 10), Times.Once);
        }

        [Fact]
        public async Task Edit_Get_WhenUserExists_ReturnsUserAndRoles()
        {
            var service = new Mock<IUserService>();
            var user = CreateUser();
            var roles = new List<Role> { new() { Id = 1, Name = "Admin" } };
            service.Setup(x => x.GetUserById(user.Id)).ReturnsAsync(user);
            service.Setup(x => x.GetAllRoles()).ReturnsAsync(roles);
            var controller = CreateController(service);

            var result = await controller.Edit(user.Id);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(user, view.Model);
            Assert.Same(roles, controller.ViewBag.Role);
        }

        [Fact]
        public async Task Edit_Get_WhenUserDoesNotExist_ReturnsNotFound()
        {
            var service = new Mock<IUserService>();
            service.Setup(x => x.GetUserById(404)).ReturnsAsync((User)null!);
            var controller = CreateController(service);

            var result = await controller.Edit(404);

            Assert.IsType<NotFoundResult>(result);
            service.Verify(x => x.GetAllRoles(), Times.Never);
        }

        [Fact]
        public async Task Edit_Post_WhenIdDoesNotMatchUser_ReturnsNotFoundWithoutUpdating()
        {
            var service = new Mock<IUserService>();
            var request = CreateUpdateRequest();
            var controller = CreateController(service);

            var result = await controller.Edit(request, request.Id + 1);

            Assert.IsType<NotFoundResult>(result);
            service.Verify(x => x.UpdateUser(It.IsAny<UpdateUserDTO>(), It.IsAny<int?>()), Times.Never);
        }

        [Fact]
        public async Task Edit_Post_WhenUpdateSucceeds_RedirectsToIndex()
        {
            var service = new Mock<IUserService>();
            var request = CreateUpdateRequest();
            service.Setup(x => x.UpdateUser(request, request.Id)).ReturnsAsync(CreateUser());
            var controller = CreateController(service);

            var result = await controller.Edit(request, request.Id);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Data updated successfully!", controller.TempData["success"]);
        }

        [Fact]
        public async Task Edit_Post_WhenUpdateFails_RedirectsToIndexWithError()
        {
            var service = new Mock<IUserService>();
            var request = CreateUpdateRequest();
            service.Setup(x => x.UpdateUser(request, request.Id)).ReturnsAsync((User)null!);
            var controller = CreateController(service);

            var result = await controller.Edit(request, request.Id);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Error Occurred!", controller.TempData["error"]);
        }

        [Fact]
        public async Task Edit_Post_WhenModelStateIsInvalid_ReturnsFormWithoutUpdating()
        {
            var service = new Mock<IUserService>();
            var request = CreateUpdateRequest();
            var controller = CreateController(service);
            controller.ModelState.AddModelError("Email", "Invalid email");

            var result = await controller.Edit(request, request.Id);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(request, view.Model);
            Assert.Equal("Error Occurred!", controller.TempData["error"]);
            service.Verify(x => x.UpdateUser(It.IsAny<UpdateUserDTO>(), It.IsAny<int?>()), Times.Never);
        }

        [Fact]
        public async Task Delete_WhenIdIsNull_ReturnsNotFoundWithoutDeleting()
        {
            var service = new Mock<IUserService>();
            var controller = CreateController(service);

            var result = await controller.Delete(null);

            Assert.IsType<NotFoundResult>(result);
            service.Verify(x => x.DeleteUser(It.IsAny<int?>()), Times.Never);
        }

        [Fact]
        public async Task Delete_WhenUserExists_DeletesAndRedirectsToIndex()
        {
            var service = new Mock<IUserService>();
            service.Setup(x => x.DeleteUser(7)).ReturnsAsync(CreateUser());
            var controller = CreateController(service);

            var result = await controller.Delete(7);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Data deleted successfully!", controller.TempData["success"]);
            service.Verify(x => x.DeleteUser(7), Times.Once);
        }

        [Fact]
        public async Task Delete_WhenUserDoesNotExist_RedirectsToIndexWithError()
        {
            var service = new Mock<IUserService>();
            service.Setup(x => x.DeleteUser(404)).ReturnsAsync((User)null!);
            var controller = CreateController(service);

            var result = await controller.Delete(404);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Error Occurred!", controller.TempData["error"]);
        }

        [Fact]
        public async Task AccountSettings_ReturnsView()
        {
            var controller = CreateController(new Mock<IUserService>());

            var result = await controller.AccountSettings();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Profile_Get_WhenUserExists_ReturnsProfileData()
        {
            var service = new Mock<IUserService>();
            var user = CreateUser();
            service.Setup(x => x.GetUserById(42)).ReturnsAsync(user);
            var controller = CreateController(service);

            var result = await controller.Profile();

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<UserProfileDTO>(view.Model);
            Assert.Equal(user.Id, model.Id);
            Assert.Equal(user.FirstName, model.FirstName);
            Assert.Equal(user.LastName, model.LastName);
            Assert.Equal(user.Email, model.Email);
            Assert.Equal(user.ProfilePicture, model.ProfilePicture);
        }

        [Fact]
        public async Task Profile_Get_WhenUserDoesNotExist_ReturnsNotFound()
        {
            var service = new Mock<IUserService>();
            service.Setup(x => x.GetUserById(42)).ReturnsAsync((User)null!);
            var controller = CreateController(service);

            var result = await controller.Profile();

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Profile_Post_WhenImageExceedsSizeLimit_ReturnsFormWithErrorWithoutUpdating()
        {
            var service = new Mock<IUserService>();
            var request = CreateProfileRequest();
            request.ImageFile = new FormFile(new MemoryStream(new byte[1024 * 1024 + 1]), 0, 1024 * 1024 + 1, "image", "profile.png");
            var controller = CreateController(service);

            var result = await controller.Profile(request);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(request, view.Model);
            Assert.Equal("File size cannot exceed 1mb!", controller.TempData["error"]);
            service.Verify(x => x.UpdateProfile(It.IsAny<UserProfileDTO>(), It.IsAny<int?>()), Times.Never);
        }

        [Fact]
        public async Task Profile_Post_WhenUpdateSucceeds_RedirectsToAccountSettings()
        {
            var service = new Mock<IUserService>();
            var request = CreateProfileRequest();
            service.Setup(x => x.UpdateProfile(request, request.Id)).ReturnsAsync(CreateUser());
            var controller = CreateController(service);

            var result = await controller.Profile(request);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("AccountSettings", redirect.ActionName);
            Assert.Equal("Data updated successfully!", controller.TempData["success"]);
            service.Verify(x => x.UpdateProfile(request, request.Id), Times.Once);
        }

        [Fact]
        public async Task Profile_Post_WhenModelStateIsInvalid_ReturnsFormWithoutUpdating()
        {
            var service = new Mock<IUserService>();
            var request = CreateProfileRequest();
            var controller = CreateController(service);
            controller.ModelState.AddModelError("Email", "Invalid email");

            var result = await controller.Profile(request);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(request, view.Model);
            Assert.Equal("Error Occurred!", controller.TempData["error"]);
            service.Verify(x => x.UpdateProfile(It.IsAny<UserProfileDTO>(), It.IsAny<int?>()), Times.Never);
        }

        [Fact]
        public async Task RemoveProfilePicture_WhenPictureIsRemoved_RedirectsToAccountSettings()
        {
            var service = new Mock<IUserService>();
            service.Setup(x => x.DeleteProfilePicture(42)).ReturnsAsync(CreateUser());
            var controller = CreateController(service);

            var result = await controller.RemoveProfilePicture();

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("AccountSettings", redirect.ActionName);
            Assert.Equal("Data updated successfully!", controller.TempData["success"]);
            service.Verify(x => x.DeleteProfilePicture(42), Times.Once);
        }

        [Fact]
        public async Task RemoveProfilePicture_WhenPictureIsAlreadyNull_RedirectsToProfileWithError()
        {
            var service = new Mock<IUserService>();
            service.Setup(x => x.DeleteProfilePicture(42)).ReturnsAsync((User)null!);
            var controller = CreateController(service);

            var result = await controller.RemoveProfilePicture();

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Profile", redirect.ActionName);
            Assert.Equal("Profile picture is already null!", controller.TempData["error"]);
        }

        [Fact]
        public async Task ChangePassword_Get_WhenUserExists_ReturnsPasswordForm()
        {
            var service = new Mock<IUserService>();
            var user = CreateUser();
            service.Setup(x => x.GetUserById(42)).ReturnsAsync(user);
            var controller = CreateController(service);

            var result = await controller.ChangePassword();

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ChangePasswordDTO>(view.Model);
            Assert.Equal(user.Id, model.UserId);
            Assert.Equal(user.Password, model.CurrentPassword);
        }

        [Fact]
        public async Task ChangePassword_Get_WhenUserDoesNotExist_ReturnsNotFound()
        {
            var service = new Mock<IUserService>();
            service.Setup(x => x.GetUserById(42)).ReturnsAsync((User)null!);
            var controller = CreateController(service);

            var result = await controller.ChangePassword();

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task ChangePassword_Post_WhenUpdateSucceeds_RedirectsToLogin()
        {
            var service = new Mock<IUserService>();
            var request = CreateChangePasswordRequest();
            service.Setup(x => x.ChangePasswordAsync(request)).ReturnsAsync(CreateUser());
            var controller = CreateController(service);

            var result = await controller.ChangePassword(request);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirect.ActionName);
            Assert.Equal("Account", redirect.ControllerName);
            Assert.Equal("Data updated successfully!", controller.TempData["success"]);
        }

        [Fact]
        public async Task ChangePassword_Post_WhenUpdateFails_ReturnsFormWithError()
        {
            var service = new Mock<IUserService>();
            var request = CreateChangePasswordRequest();
            service.Setup(x => x.ChangePasswordAsync(request)).ReturnsAsync((User)null!);
            var controller = CreateController(service);

            var result = await controller.ChangePassword(request);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(request, view.Model);
            Assert.Equal("Error Occurred!", controller.TempData["error"]);
        }

        [Fact]
        public async Task ChangePassword_Post_WhenModelStateIsInvalid_ReturnsFormWithoutUpdating()
        {
            var service = new Mock<IUserService>();
            var request = CreateChangePasswordRequest();
            var controller = CreateController(service);
            controller.ModelState.AddModelError("CurrentPassword", "Required");

            var result = await controller.ChangePassword(request);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(request, view.Model);
            Assert.Equal("Error Occurred!", controller.TempData["error"]);
            service.Verify(x => x.ChangePasswordAsync(It.IsAny<ChangePasswordDTO>()), Times.Never);
        }

        [Fact]
        public async Task ExportData_WhenFormatIsEmpty_RedirectsToIndexWithError()
        {
            var service = new Mock<IUserService>();
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
            var service = new Mock<IUserService>();
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
            var service = new Mock<IUserService>();
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

        private static UserController CreateController(Mock<IUserService> service)
        {
            var httpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    new[] { new Claim(ClaimTypes.NameIdentifier, "42") }, "test"))
            };
            var httpContextAccessor = new HttpContextAccessor { HttpContext = httpContext };

            return new UserController(service.Object, new CurrentUser(httpContextAccessor))
            {
                ControllerContext = new ControllerContext { HttpContext = httpContext },
                TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>())
            };
        }

        private static UpdateUserDTO CreateUpdateRequest() => new()
        {
            Id = 7,
            FirstName = "Alex",
            LastName = "Morgan",
            Email = "alex@example.com",
            RoleId = 1,
            IsActive = true
        };

        private static UserProfileDTO CreateProfileRequest() => new()
        {
            Id = 42,
            FirstName = "Alex",
            LastName = "Morgan",
            Email = "alex@example.com",
            ProfilePicture = "profile.png"
        };

        private static ChangePasswordDTO CreateChangePasswordRequest() => new()
        {
            UserId = 42,
            CurrentPassword = "CurrentPass1!",
            NewPassword = "NewStrongPass1!",
            ConfirmNewPassword = "NewStrongPass1!"
        };

        private static User CreateUser() => new()
        {
            Id = 42,
            FirstName = "Alex",
            LastName = "Morgan",
            Email = "alex@example.com",
            Password = "StrongPass1!",
            ProfilePicture = "profile.png"
        };
    }
}
