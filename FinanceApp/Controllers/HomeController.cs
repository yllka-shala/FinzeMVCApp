using FinanceApp.DTOs;
using FinanceApp.Helpers;
using FinanceApp.Models;
using FinanceApp.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace FinanceApp.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly INotificationService _service;
    private readonly CurrentUser _currentUser;
    private readonly IAdminReportsService _reportsService;

    public HomeController(ILogger<HomeController> logger, 
                          INotificationService service, 
                          CurrentUser currentUser, IAdminReportsService reportsService)
    {
        _logger = logger;
        _service = service;
        _currentUser = currentUser;
        _reportsService = reportsService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var customers = await _reportsService.CountCustomers();
        var users = await _reportsService.CountUsers();
        var transactionsPerMonth = await _reportsService.TotalTransactionsPerMonth();
        var dailyTransactions = await _reportsService.GetTransactionsLast7DaysAsync();
        var monthlyTransactions = await _reportsService.GetTransactionsLast7MonthsAsync();
        var InActiveUsers = await _reportsService.InActiveUsers();

        var adminReport = new AdminReportDTO
        {
            Customers = customers,
            Users = users,
            TransactionsPerMonth = transactionsPerMonth,
            DailyTransactions = dailyTransactions,
            MonthlyTransactions = monthlyTransactions,
            InActiveUsers = InActiveUsers

        };

        return View(adminReport);
    }

    public async Task<IActionResult> Logout()
    {
        await _currentUser.LogOut();
        return RedirectToAction("Login", "Account");
    }

    public IActionResult Privacy()
    {
        return View();
    }
    public IActionResult Help()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
