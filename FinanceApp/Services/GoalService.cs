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
    public class GoalService : IGoalService
    {
        private readonly FinanceAppContext _context;
        private readonly CurrentUser _currentUser;

        public GoalService(FinanceAppContext context, CurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<PaginatedList<Goal>> GetAll(string? search, int pageNumber = 1, int pageSize = 10)
        {
            var query = _context.Goals
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
                    p.User.FirstName.Contains(search) ||
                    p.User.LastName.Contains(search));
            }

            return await PaginatedList<Goal>.CreateAsync(query, pageNumber, pageSize);
        }

        public async Task<Goal> GetById(int? id)
        {
            var goal = await _context.Goals
                                     .Include(u => u.User).FirstOrDefaultAsync(g => g.Id == id);

            if (goal == null)
                return null;

            return goal;
        }

        public async Task<Goal> Create(GoalDTO model)
        {
            var exists = _context.Goals.Any(b => b.Name == model.Name && b.UserId == _currentUser.LoggedInUser());
            if (exists)
                return null;

            var create = new Goal
            {
                Name = model.Name,
                TargetAmount = model.TargetAmount,
                CurrentAmount = model.CurrentAmount,
                DueDate = model.DueDate,
                UserId = _currentUser.LoggedInUser()
            };

            await _context.Goals.AddAsync(create);
            await _context.SaveChangesAsync();

            return create;
        }

        public async Task<Goal> Update(GoalDTO update, int? id)
        {
            var existingGoal = await _context.Goals.FindAsync(id);
            if (existingGoal == null)
                return null;

            var exists = _context.Goals.Any(b => b.Name == update.Name && 
                                                 b.Id != id && b.UserId == _currentUser.LoggedInUser());
            
            if (exists)
                return null;

            existingGoal.Name = update.Name;
            existingGoal.TargetAmount = update.TargetAmount;
            existingGoal.CurrentAmount = update.CurrentAmount;
            existingGoal.DueDate = update.DueDate;
            existingGoal.UserId = _currentUser.LoggedInUser();

            if (existingGoal.CurrentAmount == existingGoal.TargetAmount)
            {
                //Send notification
                await SendNotification(existingGoal.UserId);
            }

            await _context.SaveChangesAsync();

            return existingGoal;
        }

        public async Task<Goal> Delete(int? id)
        {
            var goal = await _context.Goals.FindAsync(id);
            if (goal == null)
                return null;

            _context.Goals.Remove(goal);
            await _context.SaveChangesAsync();

            return goal;
        }

        private async Task<Notification> SendNotification(int userId)
        {

            var notification = new Notification
            {
                NotificationTypeId = (int)NotificationTypeIds.Goal_Reached,
                Message = nameof(NotificationTypeIds.Goal_Reached),
                Read = false,
                UserId = userId
            };

            await _context.Notifications.AddAsync(notification);

            return notification;
        }

        public async Task<byte[]> ExportExcel()
        {
            var exportData = await GetNotificationExportData();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Goals");

            worksheet.Cell("A1").InsertTable(exportData);

            worksheet.Column(3).Style.NumberFormat.Format = "#,##0.00$";
            worksheet.Column(4).Style.NumberFormat.Format = "#,##0.00$";

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> ExportPdf()
        {
            var exportData = await GetNotificationExportData();

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

                    page.Header().Text("Goal Report").FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);

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
                            columns.RelativeColumn();
                        });

                        // Add Header
                        table.Header(header =>
                        {
                            header.Cell().BorderBottom(2).Padding(3).Text("Id").Bold();
                            header.Cell().BorderBottom(2).Padding(3).Text("Name").Bold();
                            header.Cell().BorderBottom(2).Padding(3).Text("TargetAmount").Bold();
                            header.Cell().BorderBottom(2).Padding(3).Text("CurrentAmount").Bold();
                            header.Cell().BorderBottom(2).Padding(3).Text("Progress").Bold();
                            header.Cell().BorderBottom(2).Padding(3).Text("DueDate").Bold();
                            header.Cell().BorderBottom(2).Padding(3).Text("CreatedDate").Bold();
                            header.Cell().BorderBottom(2).Padding(3).Text("User").Bold();
                        });

                        // Add Data Rows
                        foreach (var item in exportData)
                        {
                            table.Cell().Padding(3).Text(item.Id.ToString());
                            table.Cell().Padding(3).Text(item.Name);
                            table.Cell().Padding(3).Text(item.TargetAmount.ToString() + "$");
                            table.Cell().Padding(3).Text(item.CurrentAmount.ToString() + "$");
                            table.Cell().Padding(3).Text(item.Progress.ToString("F2"));
                            table.Cell().Padding(3).Text(item.DueDate);
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

        private async Task<List<ExportGoalDataDTO>> GetNotificationExportData()
        {
            var query = _context.Goals
                                .Include(u => u.User)
                                .AsQueryable();


            if (!_currentUser.IsAdmin())
            {
                var userId = _currentUser.LoggedInUser();
                query = query.Where(t => t.UserId == userId);
            }

            var goal = await query.ToListAsync();

            return goal.Select(u => new ExportGoalDataDTO
            {
                Id = u.Id,
                Name = u.Name,
                TargetAmount = u.TargetAmount,
                CurrentAmount = u.CurrentAmount,
                Progress = u.CurrentAmount / u.TargetAmount * 100,
                DueDate = u.DueDate.ToString("dd-MM-yyyy"),
                CreatedDate = u.CreatedDate.ToString("dd-MM-yyyy"),
                User = $"{u.User!.FirstName} {u.User.LastName}"
            }).ToList();
        }
    }
}
