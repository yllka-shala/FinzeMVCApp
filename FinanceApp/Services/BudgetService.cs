using AspNetCoreGeneratedDocument;
using FinanceApp.Data;
using FinanceApp.DTOs;
using FinanceApp.Enums;
using FinanceApp.Helpers;
using FinanceApp.Models;
using Microsoft.EntityFrameworkCore;
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
    }
}
