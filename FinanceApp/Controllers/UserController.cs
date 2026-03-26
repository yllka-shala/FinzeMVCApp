using FinanceApp.DTOs;
using FinanceApp.Helpers;
using FinanceApp.Models;
using FinanceApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinanceApp.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly IUserService _service;
        private readonly CurrentUser _currentUser;

        public UserController(IUserService service, CurrentUser currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(string search, int? pageNumber)
        {
            ViewData["CurrentFilter"] = search;

            int pageSize = 10;
            var result = await _service.GetAll(search, pageNumber ?? 1, pageSize);

            return View(result);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {

            var user = await _service.GetUserById(id);
            if (user == null)
                return NotFound();

            var roles = await _service.GetAllRoles();
            ViewBag.Role = roles;

            return View(user);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(UpdateUserDTO user, int? id)
        {
            if (id != user.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                var result = await _service.UpdateUser(user, id);
                if (result == null)
                {
                    TempData["error"] = "Error Occurred!";
                    return RedirectToAction("Index");
                }

                TempData["success"] = "Data updated successfully!";
                return RedirectToAction("Index");
            }

            TempData["error"] = "Error Occurred!";
            return View(user);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var result = await _service.DeleteUser(id);
            if (result == null)
            {
                TempData["error"] = "Error Occurred!";
                return RedirectToAction("Index");
            }

            TempData["success"] = "Data deleted successfully!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> AccountSettings()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = _currentUser.LoggedInUser();

            var user = await _service.GetUserById(userId);
            if (user == null)
                return NotFound();

            var profileDTO = new UserProfileDTO
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                ProfilePicture = user.ProfilePicture
            };

            return View(profileDTO);
        }

        [HttpPost]
        public async Task<IActionResult> Profile(UserProfileDTO model)
        {
            if (ModelState.IsValid)
            {
                if (model.ImageFile != null)
                {
                    if(model.ImageFile.Length > 1 * 1024 * 1024)
                    {
                        TempData["error"] = "File size cannot exceed 1mb!";
                        return View(model);
                    }
                }

                await _service.UpdateProfile(model, model.Id);

                TempData["success"] = "Data updated successfully!";
                return RedirectToAction("AccountSettings");
            }

            TempData["error"] = "Error Occurred!";
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> RemoveProfilePicture()
        {
            var userId = _currentUser.LoggedInUser();

            var result = await _service.DeleteProfilePicture(userId);         
            if(result == null)
            {
                TempData["error"] = "Profile picture is already null!";
                return RedirectToAction("Profile");
            }
                
            TempData["success"] = "Data updated successfully!";
            return RedirectToAction("AccountSettings");
        }

        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            var userId = _currentUser.LoggedInUser();

            var user = await _service.GetUserById(userId);
            if (user == null)
                return NotFound();

            var data = new ChangePasswordDTO
            {
                UserId = user.Id,
                CurrentPassword = user.Password
            };

            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordDTO model)
        {

            if (ModelState.IsValid) 
            {
                var result = await _service.ChangePasswordAsync(model);
                if (result == null)
                {
                    TempData["error"] = "Error Occurred!";
                    return View(model);
                }

                TempData["success"] = "Data updated successfully!";
                return RedirectToAction("Login","Account");
            }

            TempData["error"] = "Error Occurred!";
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExportData(string format)
        {
            if (string.IsNullOrEmpty(format))
            {
                TempData["error"] = "Invalid format!";
                return RedirectToAction("Index");
            }

            if (format.ToLower() == "excel")
            {
                var content = await _service.ExportExcel();
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                var fileName = $"UserReport_{DateTime.Now:dd-MM-yyyy}.xlsx";

                return File(content, contentType, fileName);
            }
            else
            {
                var content = await _service.ExportPdf();
                var contentType = "application/pdf";
                var fileName = $"UserReport_{DateTime.Now:dd-MM-yyyy}.pdf";

                return File(content, contentType, fileName);
            }
        }
    }
}
