using FinanceApp.DTOs;
using FinanceApp.Services;
using FinanceApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApp.Controllers
{
    [Authorize]
    public class RecurringController : Controller
    {
        private readonly IRecurringService _service;

        public RecurringController(IRecurringService service)
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
            var frequencies = await _service.GetFrequenciesList();
            ViewBag.Frequencies = frequencies;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(RecurringDTO recurring)
        {
            if (ModelState.IsValid)
            {
                var result = await _service.Create(recurring);
                if (result == null)
                {
                    TempData["error"] = "Error Occurred!";
                    return RedirectToAction("Index");
                }

                TempData["success"] = "Data saved successfully!";
                return RedirectToAction("Index");
            }

            TempData["error"] = "Error Occurred!";
            return View(recurring);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var recurring = await _service.GetById(id);
            if (recurring == null)
                return NotFound();

            var frequencies = await _service.GetFrequenciesList();
            ViewBag.Frequencies = frequencies;

            return View(recurring);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(RecurringDTO recurring, int? id)
        {
            if (id == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                var result = await _service.Update(recurring, id);
                if (result == null)
                {
                    TempData["error"] = "Error Occurred!";
                    return RedirectToAction("Index");
                }

                TempData["success"] = "Data updated successfully!";
                return RedirectToAction("Index");
            }

            TempData["error"] = "Error Occurred!";
            return View(recurring);
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
            var fileName = $"RecurringReport_{DateTime.Now:dd-MM-yyyy}.xlsx";

            return File(content, contentType, fileName);
        }
    }
}
