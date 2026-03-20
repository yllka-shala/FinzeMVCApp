using FinanceApp.DTOs;
using FinanceApp.Services;
using FinanceApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApp.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly INotificationService _service;

        public NotificationController(INotificationService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string search, int? pageNumber)
        {
            ViewData["CurrentFilter"] = search;

            int pageSize = 10;
            var result = await _service.GetAll(search, pageNumber ?? 1, pageSize);

            return View(result);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            var notificationType = await _service.GetNotificationTypeList();
            ViewBag.NotificationType = notificationType;

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(NotificationDTO notification)
        {
            if (ModelState.IsValid)
            {
                var result = await _service.Create(notification);
                if (result == null)
                {
                    TempData["error"] = "Error Occurred!";
                    return RedirectToAction("Index");
                }

                TempData["success"] = "Data saved successfully!";
                return RedirectToAction("Index");
            }

            TempData["error"] = "Error Occurred!";
            return View(notification);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var notification = await _service.GetById(id);
            if (notification == null)
                return NotFound();

            var notificationType = await _service.GetNotificationTypeList();
            ViewBag.NotificationType = notificationType;

            return View(notification);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(NotificationDTO notification, int? id)
        {
            if (id == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                var result = await _service.Update(notification, id);
                if (result == null)
                {
                    TempData["error"] = "Error Occurred!";
                    return RedirectToAction("Index");
                }

                TempData["success"] = "Data updated successfully!";
                return RedirectToAction("Index");
            }

            TempData["error"] = "Error Occurred!";
            return View(notification);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var result = await _service.Delete(id);
            if (result == null)
            {
                TempData["error"] = "Error Occurred!";
                return RedirectToAction("Index");
            }

            TempData["success"] = "Data deleted successfully!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> ClearAll()
        {
            var result = await _service.ClearAll();
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> ExportExcel()
        {
            var content = await _service.ExportExcel();
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var fileName = $"NotificationReport_{DateTime.Now:dd-MM-yyyy}.xlsx";

            return File(content, contentType, fileName);
        }
    }
}
