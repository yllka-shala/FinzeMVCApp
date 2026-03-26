using FinanceApp.DTOs;
using FinanceApp.Services;
using FinanceApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApp.Controllers
{
    [Authorize]
    public class GoalController : Controller
    {
        private readonly IGoalService _service;

        public GoalController(IGoalService service)
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
        public async Task<IActionResult> Create(GoalDTO goal)
        {
            if (ModelState.IsValid)
            {
                var result = await _service.Create(goal);
                if (result == null)
                {
                    TempData["error"] = "Error Occurred!";
                    return RedirectToAction("Index");
                }

                TempData["success"] = "Data saved successfully!";
                return RedirectToAction("Index");
            }

            TempData["error"] = "Error Occurred!";
            return View(goal);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var goal = await _service.GetById(id);
            if (goal == null)
                return NotFound();

            return View(goal);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(GoalDTO goal, int? id)
        {
            if (id == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                var result = await _service.Update(goal, id);
                if (result == null)
                {
                    TempData["error"] = "Error Occurred!";
                    return RedirectToAction("Index");
                }

                TempData["success"] = "Data updated successfully!";
                return RedirectToAction("Index");
            }

            TempData["error"] = "Error Occurred!";
            return View(goal);
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
                var fileName = $"GoalReport_{DateTime.Now:dd-MM-yyyy}.xlsx";

                return File(content, contentType, fileName);
            }
            else
            {
                var content = await _service.ExportPdf();
                var contentType = "application/pdf";
                var fileName = $"GoalReport_{DateTime.Now:dd-MM-yyyy}.pdf";

                return File(content, contentType, fileName);
            }
        }
    }
}
