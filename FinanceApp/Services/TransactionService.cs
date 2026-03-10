using FinanceApp.Data;
using FinanceApp.DTOs;
using FinanceApp.Enums;
using FinanceApp.Helpers;
using FinanceApp.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace FinanceApp.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly FinanceAppContext _context;
        private readonly CurrentUser _currentUser;

        public TransactionService(FinanceAppContext context, CurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<PaginatedList<Transaction>> GetAll(string? search, int pageNumber = 1, int pageSize = 10)
        {
            var query = _context.Transactions
                                            .Include(c => c.Category)
                                            .Include(pm => pm.PaymentMethod)
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
                    p.Description.Contains(search) ||
                    p.Category.Name.Contains(search) ||
                    p.PaymentMethod.Name.Contains(search) ||
                    p.User.FirstName.Contains(search) ||
                    p.User.LastName.Contains(search));
            }

            return await PaginatedList<Transaction>.CreateAsync(query, pageNumber, pageSize);
        }

        public async Task<Transaction> GetById(int? id)
        {
            var transaction = await _context.Transactions
                                            .Include(c => c.Category)
                                            .Include(pm => pm.PaymentMethod)
                                            .Include(u => u.User).FirstOrDefaultAsync(t => t.Id == id);
            
            if (transaction == null)
                return null;

            return transaction;
        }

        public async Task<Transaction> Create(TransactionDTO model)
        {
            var exists = _context.Transactions.Any(b => b.Description == model.Description && 
                                                        b.UserId == _currentUser.LoggedInUser());
            
            if (exists)
                return null;

            var checkBudget = await CheckBudget(model.Amount, model.CategoryId);
            if (checkBudget == false)
                return null;

            var transaction = new Transaction
            {
                Description = model.Description,
                Amount = model.Amount,
                CategoryId = model.CategoryId,
                TransactionDate = model.TransactionDate,
                PaymentMethodId = model.PaymentMethodId,
                UserId = _currentUser.LoggedInUser()
            };

            await _context.Transactions.AddAsync(transaction);

            // Update the corresponding budget
            await UpdateBudgetOnTransactionCreate(transaction.CategoryId, transaction.Amount, transaction.UserId);

            await _context.SaveChangesAsync();

            return transaction;
        }

        public async Task<Transaction> Update(TransactionDTO update, int? id)
        {
            var existingTransaction = await _context.Transactions.FindAsync(id);
            if (existingTransaction == null)
                return null;

            var exists = _context.Transactions.Any(b => b.Description == update.Description && 
                                                        b.Id != id && b.UserId == _currentUser.LoggedInUser());
            
            if (exists)
                return null;

            existingTransaction.Description = update.Description;
            existingTransaction.Amount = update.Amount;
            existingTransaction.CategoryId = update.CategoryId;
            existingTransaction.TransactionDate = update.TransactionDate;
            existingTransaction.PaymentMethodId = update.PaymentMethodId;
            existingTransaction.UserId = _currentUser.LoggedInUser();

            await _context.SaveChangesAsync();

            return existingTransaction;
        }

        public async Task<Transaction> Delete(int? id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
                return null;

            _context.Transactions.Remove(transaction);

            // Uppdate the corresponding budget
            await UpdateBudgetOnTransactionDelete(transaction.CategoryId, transaction.Amount);

            await _context.SaveChangesAsync();

            return transaction;
        }

        public async Task<List<PaymentMethod>> GetAllPaymentMethods()
        {
            var paymentMethods = await _context.PaymentMethods.ToListAsync();
            if (paymentMethods == null)
                return null;

            return paymentMethods;
        }

        private async Task<Budget> UpdateBudgetOnTransactionCreate(int categoryId, double  amount, int userId)
        {
            var budget = await _context.Budgets.FirstOrDefaultAsync(b => b.CategoryId == categoryId);
            if (budget == null)
                return null;

            if (budget != null)
            {
                budget.CurrentSpent += amount;

                if (budget.CurrentSpent > budget.Limit)
                {
                    //Send notification
                    await SendNotification(budget.CategoryId, userId);

                    budget.CurrentSpent = budget.Limit;
                }

                budget.Remaining = budget.Limit - budget.CurrentSpent;

                if (budget.Remaining < 0)
                {
                    budget.Remaining = 0;
                }

                _context.Budgets.Update(budget);
                await _context.SaveChangesAsync();
            }

            return budget;
        }

        private async Task<Budget> UpdateBudgetOnTransactionDelete(int categoryId, double amount)
        {
            var budget = await _context.Budgets.FirstOrDefaultAsync(b => b.CategoryId == categoryId);
            if (budget == null)
                return null;

            if (budget != null)
            {
                budget.CurrentSpent -= amount;
                budget.Remaining = budget.Limit - budget.CurrentSpent;

                _context.Budgets.Update(budget);
                await _context.SaveChangesAsync();
            }

            return budget;
        }

        private async Task<Notification> SendNotification(int categoryId, int userId)
        {
            var category = await _context.Categories.Where(p => p.Id == categoryId)
                                                            .Select(p => p.Name).FirstOrDefaultAsync();

            var notification = new Notification
            {
                NotificationTypeId = (int)NotificationTypeIds.Budget_Exceeded,
                Message = category,
                Read = false,
                UserId = userId
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return notification;
        }

        private async Task<bool> CheckBudget(double amount, int categoryId)
        {
            var budget = await _context.Budgets.FirstOrDefaultAsync(b => b.CategoryId == categoryId);
            if (amount > budget.Remaining)
            {
                await SendNotification(categoryId, _currentUser.LoggedInUser());
                return false;
            }

            return true;
        }
    }
}
