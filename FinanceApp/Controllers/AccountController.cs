using FinanceApp.DTOs;
using FinanceApp.Helpers;
using FinanceApp.Models;
using FinanceApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApp.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly IAccountService _service;
        private readonly IUserService _userService;

        public AccountController(IAccountService service, IUserService userService)
        {
            _service = service;
            _userService = userService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Register()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register(RegisterUserDTO user)
        {
            if (ModelState.IsValid)
            {
                var result = await _service.RegisterUser(user);
                if (result == null)
                {
                    TempData["error"] = "Error Occurred!";
                    return View(user);
                }

                TempData["success"] = "User registered successfully!";

                return View("VerifyEmail");
            }

            return View(user);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult VerifyEmail()
        {
            return View();
        }


        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> VerifyEmail(VerificationEmailDTO verificationEmail)
        {
            if (ModelState.IsValid)
            {
                var result = await _service.VerifyEmail(verificationEmail.Token);
                if (result == null)
                {
                    TempData["error"] = "Error Occurred!";
                    return View(verificationEmail);
                }

                TempData["success"] = "Email Verified Sucessfully!";
                return RedirectToAction("Login", "Account");
            }

            TempData["error"] = "Error Occurred!";
            return View(verificationEmail);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(LoginDTO loginDto)
        {
            if (ModelState.IsValid)
            {
                var result = await _service.Login(loginDto);
                if (result.UserName == null)
                {
                    TempData["error"] = result.Message;
                    return View(loginDto);
                }

                return RedirectToAction("Index", "Home");

            }

            return View(loginDto);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> ForgotPassword()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDTO forgotPaswordDTO)
        {
            if (ModelState.IsValid)
            {
                var result = await _service.ForgotPassword(forgotPaswordDTO);
                if (result == null)
                {
                    TempData["error"] = "Error Occurred!";
                    return View(forgotPaswordDTO);
                }

                TempData["success"] = "Please check your email for password reset instruction!";
                return RedirectToAction("ResetPassword", "Account");
            }

            TempData["error"] = "Error Occurred!";
            return View(forgotPaswordDTO);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> ResetPassword()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPaswordDTO resetPaswordDTO)
        {
            if (ModelState.IsValid)
            {
                var result = await _service.ResetPassword(resetPaswordDTO);
                if (result == null)
                {
                    TempData["error"] = "Error Occurred!";
                    return View(resetPaswordDTO);
                }

                TempData["success"] = "Pasword reset successful, you can now login!";
                return RedirectToAction("Login", "Account");
            }

            TempData["error"] = "Error Occurred!";
            return View(resetPaswordDTO);
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
