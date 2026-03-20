using FinanceApp.DTOs;
using FinanceApp.Services;
using FinanceApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApp.Controllers
{
    [Authorize]
    public class FinancialAccountController : Controller
    {
        private readonly IFinancialAccountService _service;

        public FinancialAccountController(IFinancialAccountService service)
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
            var accountTypes = await _service.GetAccountTypeList();
            ViewBag.AccountTypes = accountTypes;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(FinancialAccountDTO financialAccount)
        {
            if (ModelState.IsValid)
            {
                var result = await _service.Create(financialAccount);
                if (result == null)
                {
                    TempData["error"] = "Error Occurred!";
                    return RedirectToAction("Index");
                }

                TempData["success"] = "Data saved successfully!";
                return RedirectToAction("Index");
            }

            TempData["error"] = "Error Occurred!";
            return View(financialAccount);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var financialAccount = await _service.GetById(id);
            if (financialAccount == null)
                return NotFound();

            var accountTypes = await _service.GetAccountTypeList();
            ViewBag.AccountTypes = accountTypes;

            return View(financialAccount);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(FinancialAccountDTO financialAccount, int? id)
        {
            if (id == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                var result = await _service.Update(financialAccount, id);
                if (result == null)
                {
                    TempData["error"] = "Error Occurred!";
                    return RedirectToAction("Index");
                }

                TempData["success"] = "Data updated successfully!";
                return RedirectToAction("Index");
            }

            TempData["error"] = "Error Occurred!";
            return View(financialAccount);
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
            var fileName = $"FinancialAccountReport_{DateTime.Now:dd-MM-yyyy}.xlsx";

            return File(content, contentType, fileName);
        }
    }
}
