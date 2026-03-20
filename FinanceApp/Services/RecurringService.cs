using ClosedXML.Excel;
using FinanceApp.Data;
using FinanceApp.DTOs;
using FinanceApp.Enums;
using FinanceApp.Helpers;
using FinanceApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FinanceApp.Services
{
    public class RecurringService : IRecurringService
    {
        private readonly FinanceAppContext _context;
        private readonly CurrentUser _currentUser;

        public RecurringService(FinanceAppContext context, CurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<PaginatedList<Recurring>> GetAll(string? search, int pageNumber = 1, int pageSize = 10)
        {
            var query = _context.Recurrings
                                        .Include(f => f.Frequency)
                                        .Include(u => u.User)
                                        .AsNoTracking();

            if (query == null)
                return null;

            if (!_currentUser.IsAdmin())
            {
                int userId = _currentUser.LoggedInUser();
                query = query.Where(u => u.UserId == userId);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p =>
                    p.Name.Contains(search) ||
                    p.Frequency.Name.Contains(search) ||
                    p.User.FirstName.Contains(search) ||
                    p.User.LastName.Contains(search));
            }

            return await PaginatedList<Recurring>.CreateAsync(query, pageNumber, pageSize);
        }

        public async Task<Recurring> GetById(int? id)
        {
            var recurring = await _context.Recurrings
                                          .Include(f => f.Frequency)
                                          .Include(u => u.User).FirstOrDefaultAsync(r => r.Id == id);
            
            if (recurring == null)
                return null;

            return recurring;
        }

        public async Task<Recurring> Create(RecurringDTO model)
        {
            var exists = _context.Recurrings.Any(b => b.Name == model.Name && b.UserId == _currentUser.LoggedInUser());
            if (exists)
                return null;

            var create = new Recurring
            {
               Name = model.Name,
               Amount = model.Amount,
               DueDate = model.DueDate,
               FrequencyId = model.FrequencyId,
               UserId = _currentUser.LoggedInUser()
            };

            await _context.Recurrings.AddAsync(create);
            await _context.SaveChangesAsync();

            return create;
        }

        public async Task<Recurring> Update(RecurringDTO update, int? id)
        {
            var existingRecuring = await _context.Recurrings.FindAsync(id);
            if (existingRecuring == null)
                return null;

            var exists = _context.Recurrings.Any(b => b.Name == update.Name && b.Id != id && b.UserId == _currentUser.LoggedInUser());
            if (exists)
                return null;

            existingRecuring.Name = update.Name;
            existingRecuring.Amount = update.Amount;
            existingRecuring.DueDate = update.DueDate;
            existingRecuring.FrequencyId = update.FrequencyId;
            existingRecuring.UserId = _currentUser.LoggedInUser();

            await _context.SaveChangesAsync();

            return existingRecuring;
        }

        public async Task<Recurring> Delete(int? id)
        {
            var notification = await _context.Recurrings.FindAsync(id);
            if (notification == null)
                return null;

            _context.Recurrings.Remove(notification);
            await _context.SaveChangesAsync();

            return notification;
        }

        public async Task<List<Frequency>> GetFrequenciesList()
        {
            var frequencies = await _context.Frequencies.ToListAsync();
            if (frequencies == null)
                return null;

            return frequencies;
        }

        public async Task<byte[]> ExportExcel()
        {
            var recurrings = await _context.Recurrings
                                        .Include(f => f.Frequency)
                                        .Include(u => u.User)
                                        .ToListAsync();

            if (!_currentUser.IsAdmin())
            {
                var userId = _currentUser.LoggedInUser();
                recurrings = recurrings.Where(t => t.UserId == userId).ToList();
            }

            var exportData = recurrings.Select(u => new
            {
                Id = u.Id,
                Name = u.Name,
                Amount = $"{u.Amount}$",
                DueDate = u.DueDate.ToString("dd-MM-yyyy"),
                Frequency = u.Frequency!.Name,
                CreatedDate = u.CreatedDate.ToString("dd-MM-yyyy"),
                User = $"{u.User!.FirstName} {u.User.LastName}"
            }).ToList();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Recurrings");

            worksheet.Cell("A1").InsertTable(exportData);

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}
