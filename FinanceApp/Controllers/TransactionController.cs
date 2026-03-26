using FinanceApp.DTOs;
using FinanceApp.Helpers;
using FinanceApp.Models;
using FinanceApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;

namespace FinanceApp.Controllers
{
    [Authorize]
    public class TransactionController : Controller
    {
        private readonly ITransactionService _service;
        private readonly ICategoryService _categoryService;

        public TransactionController(ITransactionService service, ICategoryService categoryService)
        {
            _service = service;
            _categoryService = categoryService;
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
            var categories = await _categoryService.GetCategories();
            ViewBag.Categories = categories;

            var paymentMethods = await _service.GetAllPaymentMethods();
            ViewBag.PaymentMethods = paymentMethods;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(TransactionDTO transaction)
        {
            if (ModelState.IsValid)
            {
                var result = await _service.Create(transaction); 
                if(result == null)
                {
                    TempData["error"] = "Error Occurred!";
                    return RedirectToAction("Index");
                }                

                TempData["success"] = "Data saved successfully!";
                return RedirectToAction("Index");
            }

            TempData["error"] = "Error Occurred!";
            return View(transaction);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var transaction = await _service.GetById(id);
            if (transaction == null)
                return NotFound();

            var categories = await _categoryService.GetCategories();
            ViewBag.Categories = categories;

            var paymentMethods = await _service.GetAllPaymentMethods();
            ViewBag.PaymentMethods = paymentMethods;

            return View(transaction);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int? id, TransactionDTO transaction)
        {
            if (id == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                var result = await _service.Update(transaction, id);
                if (result == null)
                {
                    TempData["error"] = "Error Occurred!";
                    return RedirectToAction("Index");
                }

                TempData["success"] = "Data updated successfully!";
                return RedirectToAction("Index");
            }

            TempData["error"] = "Error Occurred!";
            return View(transaction);
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
                var fileName = $"TransactionReport_{DateTime.Now:dd-MM-yyyy}.xlsx";

                return File(content, contentType, fileName);
            }
            else
            {
                var content = await _service.ExportPdf();
                var contentType = "application/pdf";
                var fileName = $"TransactionReport_{DateTime.Now:dd-MM-yyyy}.pdf";

                return File(content, contentType, fileName);
            }
        }
    }
}
