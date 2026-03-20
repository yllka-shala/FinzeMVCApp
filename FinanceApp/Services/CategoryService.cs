using ClosedXML.Excel;
using FinanceApp.Data;
using FinanceApp.DTOs;
using FinanceApp.Enums;
using FinanceApp.Helpers;
using FinanceApp.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Bcpg;
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
            var categories = _context.Categories
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

            if (!_currentUser.IsAdmin())
            {
                var userId = _currentUser.LoggedInUser();
                categories = categories.Where(t => t.UserId == userId);
            }

            var exportData = categories.Select(u => new
            {
                Id = u.Id,
                Name = u.Name,
                Date = u.Date.ToString("dd-MM-yyyy"),
                Username = u.Username
            }).ToList();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Category");

            worksheet.Cell("A1").InsertTable(exportData);

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}
