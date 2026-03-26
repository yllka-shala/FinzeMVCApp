using ClosedXML.Excel;
using FinanceApp.Data;
using FinanceApp.DTOs;
using FinanceApp.Enums;
using FinanceApp.Helpers;
using FinanceApp.Models;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
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
            var exportData = await GetRecurringExportData();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Recurrings");

            worksheet.Cell("A1").InsertTable(exportData);

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> ExportPdf()
        {
            var exportData = await GetRecurringExportData();

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

                    page.Header().Text("Recurring Report").FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);

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
                            header.Cell().BorderBottom(2).Padding(3).Text("Name").Bold();
                            header.Cell().BorderBottom(2).Padding(3).Text("Amount").Bold();
                            header.Cell().BorderBottom(2).Padding(3).Text("DueDate").Bold();
                            header.Cell().BorderBottom(2).Padding(3).Text("Frequency").Bold();
                            header.Cell().BorderBottom(2).Padding(3).Text("CreatedDate").Bold();
                            header.Cell().BorderBottom(2).Padding(3).Text("User").Bold();
                        });

                        // Add Data Rows
                        foreach (var item in exportData)
                        {
                            table.Cell().Padding(3).Text(item.Id.ToString());
                            table.Cell().Padding(3).Text(item.Name);
                            table.Cell().Padding(3).Text(item.Amount);
                            table.Cell().Padding(3).Text(item.DueDate);
                            table.Cell().Padding(3).Text(item.Frequency);
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

        private async Task<List<ExportRecurringDataDTO>> GetRecurringExportData()
        {
            var query = _context.Recurrings
                                .Include(f => f.Frequency)
                                .Include(u => u.User)
                                .AsQueryable();


            if (!_currentUser.IsAdmin())
            {
                var userId = _currentUser.LoggedInUser();
                query = query.Where(t => t.UserId == userId);
            }

            var recurrings = await query.ToListAsync();

            return recurrings.Select(u => new ExportRecurringDataDTO
            {
                Id = u.Id,
                Name = u.Name,
                Amount = $"{u.Amount}$",
                DueDate = u.DueDate.ToString("dd-MM-yyyy"),
                Frequency = u.Frequency!.Name,
                CreatedDate = u.CreatedDate.ToString("dd-MM-yyyy"),
                User = $"{u.User!.FirstName} {u.User.LastName}"
            }).ToList();
        }
    }
}
