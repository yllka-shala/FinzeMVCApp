using FinanceApp.DTOs;
using FinanceApp.Models;
using FinanceApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApp.Controllers
{
    [Authorize]
    public class BillReminderController : Controller
    {
        private readonly IBillReminderService _service;

        public BillReminderController(IBillReminderService service)
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
        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(BillReminderDTO billReminder)
        {
            if (ModelState.IsValid)
            {
                var result = await _service.Create(billReminder);
                if (result == null)
                {
                    TempData["error"] = "Error Occurred!";
                    return RedirectToAction("Index");
                }

                TempData["success"] = "Data saved successfully!";
                return RedirectToAction("Index");
            }

            TempData["error"] = "Error Occurred!";
            return View(billReminder);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var result = await _service.GetById(id);
            if (result == null)
                return NotFound();

            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(BillReminderDTO billReminder, int? id)
        {
            if (id == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                var result = await _service.Update(billReminder, id);
                if (result == null)
                {
                    TempData["error"] = "Error Occurred!";
                    return RedirectToAction("Index");
                }

                TempData["success"] = "Data updated successfully!";
                return RedirectToAction("Index");
            }

            TempData["error"] = "Error Occurred!";
            return View(billReminder);
        }

        [HttpGet]
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

        [HttpPost]
        public async Task<IActionResult> ExportExcel()
        {
            var content = await _service.ExportExcel();
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var fileName = $"BillReminderReport_{DateTime.Now:dd-MM-yyyy}.xlsx";

            return File(content, contentType, fileName);
        }
    }
}
