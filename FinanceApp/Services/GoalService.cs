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
            var goals = await _context.Goals
                                    .Include(u => u.User)
                                    .ToListAsync();

            if (!_currentUser.IsAdmin())
            {
                var userId = _currentUser.LoggedInUser();
                goals = goals.Where(t => t.UserId == userId).ToList();
            }

            var exportData = goals.Select(u => new
            {
                Id = u.Id,
                Name = u.Name,
                TargetAmount = $"{u.TargetAmount}$",
                CurrentAmount = $"{u.CurrentAmount}$",
                Progress = $"{u.Progress.ToString("F2")}%",
                DueDate = u.DueDate.ToString("dd-MM-yyyy"),
                CreatedDate = u.CreatedDate.ToString("dd-MM-yyyy"),
                User = $"{u.User!.FirstName} {u.User.LastName}"
            }).ToList();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Goals");

            worksheet.Cell("A1").InsertTable(exportData);

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}
