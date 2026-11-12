using FinanceApp.Controllers;
using FinanceApp.DTOs;
using FinanceApp.Models;
using FinanceApp.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace FinanceApp.Tests.ControllersTests
{
    public class AccountControllerTests
    {
        [Fact]
        public async Task Register_Get_ReturnsView()
        {
            var controller = CreateController(new Mock<IAccountService>());

            var result = await controller.Register();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Register_Post_WhenRegistrationSucceeds_ReturnsVerifyEmailView()
        {
            var service = new Mock<IAccountService>();
            var user = CreateUser();
            var request = new RegisterUserDTO
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Password = user.Password
            };
            service.Setup(x => x.RegisterUser(request)).ReturnsAsync(user);
            var controller = CreateController(service);

            var result = await controller.Register(request);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal("VerifyEmail", view.ViewName);
            Assert.Equal("User registered successfully!", controller.TempData["success"]);
            service.Verify(x => x.RegisterUser(request), Times.Once);
        }

        [Fact]
        public async Task Register_Post_WhenRegistrationFails_ReturnsFormWithError()
        {
            var service = new Mock<IAccountService>();
            var request = CreateRegisterRequest();
            service.Setup(x => x.RegisterUser(request)).ReturnsAsync((User)null!);
            var controller = CreateController(service);

            var result = await controller.Register(request);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(request, view.Model);
            Assert.Equal("Error Occurred!", controller.TempData["error"]);
        }

        [Fact]
        public async Task Register_Post_WhenModelStateIsInvalid_ReturnsFormWithoutCallingService()
        {
            var service = new Mock<IAccountService>();
            var request = CreateRegisterRequest();
            var controller = CreateController(service);
            controller.ModelState.AddModelError("Email", "Invalid email");

            var result = await controller.Register(request);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(request, view.Model);
            service.Verify(x => x.RegisterUser(It.IsAny<RegisterUserDTO>()), Times.Never);
        }

        [Fact]
        public void VerifyEmail_Get_ReturnsView()
        {
            var controller = CreateController(new Mock<IAccountService>());

            var result = controller.VerifyEmail();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task VerifyEmail_Post_WhenVerificationSucceeds_RedirectsToLogin()
        {
            var service = new Mock<IAccountService>();
            var request = new VerificationEmailDTO { Token = "verification-token" };
            service.Setup(x => x.VerifyEmail(request.Token))
                .ReturnsAsync(new VerificationEmailResponseDTO { Message = "Verified" });
            var controller = CreateController(service);

            var result = await controller.VerifyEmail(request);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirect.ActionName);
            Assert.Equal("Account", redirect.ControllerName);
            Assert.Equal("Email Verified Sucessfully!", controller.TempData["success"]);
        }

        [Fact]
        public async Task VerifyEmail_Post_WhenVerificationFails_ReturnsFormWithError()
        {
            var service = new Mock<IAccountService>();
            var request = new VerificationEmailDTO { Token = "invalid-token" };
            service.Setup(x => x.VerifyEmail(request.Token))
                .ReturnsAsync((VerificationEmailResponseDTO)null!);
            var controller = CreateController(service);

            var result = await controller.VerifyEmail(request);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(request, view.Model);
            Assert.Equal("Error Occurred!", controller.TempData["error"]);
        }

        [Fact]
        public async Task VerifyEmail_Post_WhenModelStateIsInvalid_ReturnsFormWithoutCallingService()
        {
            var service = new Mock<IAccountService>();
            var request = new VerificationEmailDTO { Token = "verification-token" };
            var controller = CreateController(service);
            controller.ModelState.AddModelError("Token", "Invalid token");

            var result = await controller.VerifyEmail(request);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(request, view.Model);
            Assert.Equal("Error Occurred!", controller.TempData["error"]);
            service.Verify(x => x.VerifyEmail(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Login_Get_ReturnsView()
        {
            var controller = CreateController(new Mock<IAccountService>());

            var result = await controller.Login();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Login_Post_WhenCredentialsAreValid_RedirectsToHome()
        {
            var service = new Mock<IAccountService>();
            var request = CreateLoginRequest();
            service.Setup(x => x.Login(request)).ReturnsAsync(new LoginResponseDTO
            {
                UserId = 1,
                UserName = "Alex",
                Message = "Login successful",
                Token = "auth-token"
            });
            var controller = CreateController(service);

            var result = await controller.Login(request);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Home", redirect.ControllerName);
            service.Verify(x => x.Login(request), Times.Once);
        }

        [Fact]
        public async Task Login_Post_WhenCredentialsAreInvalid_ReturnsFormWithServiceMessage()
        {
            var service = new Mock<IAccountService>();
            var request = CreateLoginRequest();
            service.Setup(x => x.Login(request)).ReturnsAsync(new LoginResponseDTO
            {
                UserName = null!,
                Message = "Invalid email or password"
            });
            var controller = CreateController(service);

            var result = await controller.Login(request);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(request, view.Model);
            Assert.Equal("Invalid email or password", controller.TempData["error"]);
        }

        [Fact]
        public async Task Login_Post_WhenModelStateIsInvalid_ReturnsFormWithoutCallingService()
        {
            var service = new Mock<IAccountService>();
            var request = CreateLoginRequest();
            var controller = CreateController(service);
            controller.ModelState.AddModelError("Email", "Invalid email");

            var result = await controller.Login(request);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(request, view.Model);
            service.Verify(x => x.Login(It.IsAny<LoginDTO>()), Times.Never);
        }

        [Fact]
        public async Task ForgotPassword_Get_ReturnsView()
        {
            var controller = CreateController(new Mock<IAccountService>());

            var result = await controller.ForgotPassword();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task ForgotPassword_Post_WhenRequestSucceeds_RedirectsToResetPassword()
        {
            var service = new Mock<IAccountService>();
            var request = new ForgotPasswordDTO { Email = "alex@example.com" };
            service.Setup(x => x.ForgotPassword(request)).ReturnsAsync(CreateUser());
            var controller = CreateController(service);

            var result = await controller.ForgotPassword(request);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ResetPassword", redirect.ActionName);
            Assert.Equal("Account", redirect.ControllerName);
            Assert.Equal("Please check your email for password reset instruction!", controller.TempData["success"]);
        }

        [Fact]
        public async Task ForgotPassword_Post_WhenRequestFails_ReturnsFormWithError()
        {
            var service = new Mock<IAccountService>();
            var request = new ForgotPasswordDTO { Email = "alex@example.com" };
            service.Setup(x => x.ForgotPassword(request)).ReturnsAsync((User)null!);
            var controller = CreateController(service);

            var result = await controller.ForgotPassword(request);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(request, view.Model);
            Assert.Equal("Error Occurred!", controller.TempData["error"]);
        }

        [Fact]
        public async Task ForgotPassword_Post_WhenModelStateIsInvalid_ReturnsFormWithoutCallingService()
        {
            var service = new Mock<IAccountService>();
            var request = new ForgotPasswordDTO { Email = "invalid-email" };
            var controller = CreateController(service);
            controller.ModelState.AddModelError("Email", "Invalid email");

            var result = await controller.ForgotPassword(request);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(request, view.Model);
            Assert.Equal("Error Occurred!", controller.TempData["error"]);
            service.Verify(x => x.ForgotPassword(It.IsAny<ForgotPasswordDTO>()), Times.Never);
        }

        [Fact]
        public async Task ResetPassword_Get_ReturnsView()
        {
            var controller = CreateController(new Mock<IAccountService>());

            var result = await controller.ResetPassword();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task ResetPassword_Post_WhenResetSucceeds_RedirectsToLogin()
        {
            var service = new Mock<IAccountService>();
            var request = CreateResetPasswordRequest();
            service.Setup(x => x.ResetPassword(request)).ReturnsAsync(CreateUser());
            var controller = CreateController(service);

            var result = await controller.ResetPassword(request);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirect.ActionName);
            Assert.Equal("Account", redirect.ControllerName);
            Assert.Equal("Pasword reset successful, you can now login!", controller.TempData["success"]);
        }

        [Fact]
        public async Task ResetPassword_Post_WhenResetFails_ReturnsFormWithError()
        {
            var service = new Mock<IAccountService>();
            var request = CreateResetPasswordRequest();
            service.Setup(x => x.ResetPassword(request)).ReturnsAsync((User)null!);
            var controller = CreateController(service);

            var result = await controller.ResetPassword(request);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(request, view.Model);
            Assert.Equal("Error Occurred!", controller.TempData["error"]);
        }

        [Fact]
        public async Task ResetPassword_Post_WhenModelStateIsInvalid_ReturnsFormWithoutCallingService()
        {
            var service = new Mock<IAccountService>();
            var request = CreateResetPasswordRequest();
            var controller = CreateController(service);
            controller.ModelState.AddModelError("Token", "Invalid token");

            var result = await controller.ResetPassword(request);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(request, view.Model);
            Assert.Equal("Error Occurred!", controller.TempData["error"]);
            service.Verify(x => x.ResetPassword(It.IsAny<ResetPaswordDTO>()), Times.Never);
        }

        [Fact]
        public void AccessDenied_Get_ReturnsView()
        {
            var controller = CreateController(new Mock<IAccountService>());

            var result = controller.AccessDenied();

            Assert.IsType<ViewResult>(result);
        }

        private static AccountController CreateController(Mock<IAccountService> service)
        {
            return new AccountController(service.Object, Mock.Of<IUserService>())
            {
                TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>())
            };
        }

        private static RegisterUserDTO CreateRegisterRequest() => new()
        {
            FirstName = "Alex",
            LastName = "Morgan",
            Email = "alex@example.com",
            Password = "StrongPass1!"
        };

        private static LoginDTO CreateLoginRequest() => new()
        {
            Email = "alex@example.com",
            Password = "StrongPass1!"
        };

        private static ResetPaswordDTO CreateResetPasswordRequest() => new()
        {
            Token = "reset-token",
            NewPassword = "NewStrongPass1!",
            ConfirmNewPassword = "NewStrongPass1!"
        };

        private static User CreateUser() => new()
        {
            Id = 1,
            FirstName = "Alex",
            LastName = "Morgan",
            Email = "alex@example.com",
            Password = "StrongPass1!"
        };
    }
}
