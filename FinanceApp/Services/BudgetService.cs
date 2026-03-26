using AspNetCoreGeneratedDocument;
using ClosedXML.Excel;
using FinanceApp.Data;
using FinanceApp.DTOs;
using FinanceApp.Enums;
using FinanceApp.Helpers;
using FinanceApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Security.Claims;

namespace FinanceApp.Services
{
    public class BudgetService : IBudgetService
    {
        private readonly FinanceAppContext _context;
        private readonly CurrentUser _currentUser;

        public BudgetService(FinanceAppContext context, CurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<PaginatedList<Budget>> GetAll(string? search, int pageNumber = 1, int pageSize = 10)
        {
            var query = _context.Budgets
                                        .Include(c => c.Category)
                                        .Include(u => u.User)
                                        .AsNoTracking();

            if (query == null)
                return null;

            if (!_currentUser.IsAdmin())
            {
                var userId = _currentUser.LoggedInUser();
                query = query.Where(t => t.UserId == userId);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p =>
                    p.Category.Name.Contains(search) ||
                    p.User.FirstName.Contains(search) ||
                    p.User.LastName.Contains(search));
            }

            return await PaginatedList<Budget>.CreateAsync(query, pageNumber, pageSize);
        }

        public async Task<Budget> GetById(int? id)
        {
            var budget = await _context.Budgets
                                       .Include(c => c.Category)
                                       .Include(u => u.User).FirstOrDefaultAsync(b => b.Id == id);

            if (budget == null)
                return null;

            return budget;
        }

        public async Task<Budget> Create(BudgetDTO model)
        {
            var exists = _context.Budgets.Any(b => b.CategoryId == model.CategoryId &&
                                                   b.UserId == _currentUser.LoggedInUser());

            if (exists)
                return null;

            var create = new Budget
            {
                CategoryId = model.CategoryId,
                Limit = model.Limit,
                CurrentSpent = model.CurrentSpent == null ? 0 : model.CurrentSpent,
                Remaining = model.Remaining,
                UserId = _currentUser.LoggedInUser()
            };

            await _context.Budgets.AddAsync(create);
            await _context.SaveChangesAsync();

            return create;
        }

        public async Task<Budget> Update(BudgetDTO update, int? id)
        {
            var existingBudget = await _context.Budgets.FindAsync(id);
            if (existingBudget == null)
                return null;

            var exists = _context.Budgets.Any(b => b.CategoryId == update.CategoryId &&
                                                   b.Id != id && b.UserId == _currentUser.LoggedInUser());

            if (exists)
                return null;

            existingBudget.CategoryId = update.CategoryId;
            existingBudget.Limit = update.Limit;
            existingBudget.CurrentSpent = update.CurrentSpent == null ? 0 : update.CurrentSpent;
            existingBudget.Remaining = update.Remaining;
            existingBudget.UserId = _currentUser.LoggedInUser();

            await _context.SaveChangesAsync();

            return existingBudget;
        }

        public async Task<Budget> Delete(int? id)
        {
            var budget = await _context.Budgets.FindAsync(id);
            if (budget == null)
                return null;

            _context.Budgets.Remove(budget);
            await _context.SaveChangesAsync();

            return budget;
        }

        public async Task<byte[]> ExportExcel()
        {
            var exportData = await GetBudgetExportData();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Budgets");

            worksheet.Cell("A1").InsertTable(exportData);

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> ExportPdf()
        {
            var exportData = await GetBudgetExportData();

            QuestPDF.Settings.License = LicenseType.Community;

            // Create the document
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header().Text("Budget Report").FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);

                    page.Content().PaddingVertical(30).Table(table =>
                    {
                        // Define columns
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(50); 
                            columns.RelativeColumn(); 
                            columns.RelativeColumn(); 
                            columns.RelativeColumn(); 
                            columns.RelativeColumn();
                            columns.RelativeColumn(); 
                            columns.RelativeColumn();
                        });

                        // Add Header
                        table.Header(header =>
                        {
                            header.Cell().BorderBottom(2).Padding(3).Text("Id").Bold();
                            header.Cell().BorderBottom(2).Padding(3).Text("Category").Bold();
                            header.Cell().BorderBottom(2).Padding(3).Text("Limit").Bold();
                            header.Cell().BorderBottom(2).Padding(3).Text("CurrentSpent").Bold();
                            header.Cell().BorderBottom(2).Padding(3).Text("Remaining").Bold();
                            header.Cell().BorderBottom(2).Padding(3).Text("CreatedDate").Bold();
                            header.Cell().BorderBottom(2).Padding(3).Text("User").Bold();
                        });

                        // Add Data Rows
                        foreach (var item in exportData)
                        {
                            table.Cell().Padding(3).Text(item.Id.ToString());
                            table.Cell().Padding(3).Text(item.Category);
                            table.Cell().Padding(3).Text(item.Limit);
                            table.Cell().Padding(3).Text(item.CurrentSpent);
                            table.Cell().Padding(3).Text(item.Remaining);
                            table.Cell().Padding(3).Text(item.CreatedDate);
                            table.Cell().Padding(3).Text(item.User);
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                    });
                });
            });

            return document.GeneratePdf();
        }

        private async Task<List<ExportBudgetDTO>> GetBudgetExportData()
        {
            var query = _context.Budgets
                .Include(c => c.Category)
                .Include(u => u.User)
                .AsQueryable();

            if (!_currentUser.IsAdmin())
            {
                var userId = _currentUser.LoggedInUser();
                query = query.Where(t => t.UserId == userId);
            }

            var budgets = await query.ToListAsync();

            return budgets.Select(u => new ExportBudgetDTO
            {
                Id = u.Id,
                Category = u.Category?.Name ?? "N/A",
                Limit = $"{u.Limit}$",
                CurrentSpent = $"{u.CurrentSpent}$",
                Remaining = $"{u.Remaining}$",
                CreatedDate = u.CreatedDate.ToString("dd-MM-yyyy"),
                User = u.User != null ? $"{u.User.FirstName} {u.User.LastName}" : "Unknown"
            }).ToList();
        }
    }
}
