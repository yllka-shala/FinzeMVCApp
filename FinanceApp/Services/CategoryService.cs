using ClosedXML.Excel;
using FinanceApp.Data;
using FinanceApp.DTOs;
using FinanceApp.Enums;
using FinanceApp.Helpers;
using FinanceApp.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Bcpg;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace FinanceApp.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly FinanceAppContext _context;
        private readonly CurrentUser _currentUser;

        public CategoryService(FinanceAppContext context, CurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<PaginatedList<CategoryWithUserDto>> GetAll(string? search, int pageNumber = 1, int pageSize = 10)
        {

            var query = _context.Categories
                                        .Join(
                                            _context.Users,
                                            c => c.UserId,
                                            u => u.Id,
                                            (c, u) => new CategoryWithUserDto
                                            {
                                                Id = c.Id,
                                                Name = c.Name,
                                                Date = c.Date,
                                                UserId = c.UserId,
                                                Username = u.FirstName + " " + u.LastName
                                            }
                                        );


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
                    p.Name.Contains(search) ||
                    p.Username.Contains(search));
            }

            return await PaginatedList<CategoryWithUserDto>.CreateAsync(query, pageNumber, pageSize);
        }

        public async Task<List<CategoryWithUserDto>> GetCategories()
        {
            var userId = _currentUser.LoggedInUser();

            var category = await _context.Categories
                                         .Join(
                                              _context.Users,
                                              c => c.UserId,
                                              u => u.Id,
                                              (c, u) => new CategoryWithUserDto
                                              {
                                                  Id = c.Id,
                                                  Name = c.Name,
                                                  Date = c.Date,
                                                  UserId = c.UserId,
                                                  Username = u.FirstName + " " + u.LastName
                                              }
                                         )
                                         .Where(u => u.UserId == userId)
                                         .ToListAsync();

            if (category == null)
                return null;

            return category;
        }

        public async Task<Category> GetById(int? id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return null;

            return category;
        }

        public async Task<Category> Create(CategoryDTO model)
        {
            var exists = _context.Categories.Any(c => c.Name == model.Name && 
                                                      c.UserId == _currentUser.LoggedInUser());

            if (exists)
                return null;

            var create = new Category
            {
                Name = model.Name,
                UserId = _currentUser.LoggedInUser()
            };

            await _context.Categories.AddAsync(create);
            await _context.SaveChangesAsync();

            return create;
        }

        public async Task<Category> Update(CategoryDTO category, int? id)
        {
            var existingCategory = await _context.Categories.FindAsync(id);
            if (existingCategory == null)
                return null;

            var exists = _context.Categories.Any(c => c.Name == category.Name && 
                                                      c.Id != id && c.UserId == _currentUser.LoggedInUser());
            
            if (exists)
                return null;

            existingCategory.Name = category.Name;
            existingCategory.UserId = _currentUser.LoggedInUser();

            await _context.SaveChangesAsync();
            return existingCategory;
        }

        public async Task<Category> Delete(int? id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return null;

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return category;
        }

        public async Task<byte[]> ExportExcel()
        {
            var exportData = await GetCategoryExportData();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Category");

            worksheet.Cell("A1").InsertTable(exportData);

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> ExportPdf()
        {
            var exportData = await GetCategoryExportData();

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

                    page.Header().Text("Category Report").FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);

                    page.Content().PaddingVertical(30).Table(table =>
                    {
                        // Define columns
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(50);
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        // Add Header
                        table.Header(header =>
                        {
                            header.Cell().BorderBottom(2).Padding(3).Text("Id").Bold();
                            header.Cell().BorderBottom(2).Padding(3).Text("Name").Bold();
                            header.Cell().BorderBottom(2).Padding(3).Text("Date").Bold();
                            header.Cell().BorderBottom(2).Padding(3).Text("User").Bold();
                        });

                        // Add Data Rows
                        foreach (var item in exportData)
                        {
                            table.Cell().Padding(3).Text(item.Id.ToString());
                            table.Cell().Padding(3).Text(item.Name);
                            table.Cell().Padding(3).Text(item.Date);
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

        private async Task<List<ExportCategoryDataDTO>> GetCategoryExportData()
        {
            var query = _context.Categories
                                       .Join(
                                           _context.Users,
                                           c => c.UserId,
                                           u => u.Id,
                                           (c, u) => new CategoryWithUserDto
                                           {
                                               Id = c.Id,
                                               Name = c.Name,
                                               Date = c.Date,
                                               UserId = c.UserId,
                                               Username = u.FirstName + " " + u.LastName
                                           }
                                       ).AsQueryable();


            if (!_currentUser.IsAdmin())
            {
                var userId = _currentUser.LoggedInUser();
                query = query.Where(t => t.UserId == userId);
            }

            var category = await query.ToListAsync();

            return category.Select(u => new ExportCategoryDataDTO
            {
                Id = u.Id,
                Name = u.Name,
                Date = u.Date.ToString("dd-MM-yyyy"),
                User = u.Username
            }).ToList();
        }
    }
}
