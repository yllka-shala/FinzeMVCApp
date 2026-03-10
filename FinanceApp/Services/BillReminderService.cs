using FinanceApp.Data;
using FinanceApp.DTOs;
using FinanceApp.Enums;
using FinanceApp.Helpers;
using FinanceApp.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;

namespace FinanceApp.Services
{
    public class BillReminderService : IBillReminderService
    {
        private readonly FinanceAppContext _context;
        private readonly CurrentUser _currentUser;


        public BillReminderService(FinanceAppContext context, CurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<PaginatedList<BillReminder>> GetAll(string? search, int pageNumber = 1, int pageSize = 10)
        {
            var query = _context.BillReminders
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
                    p.Name.Contains(search) ||
                    p.User.FirstName.Contains(search) ||
                    p.User.LastName.Contains(search));
            }

            return await PaginatedList<BillReminder>.CreateAsync(query, pageNumber, pageSize);
        }

        public async Task<BillReminder> GetById(int? id)
        {
            var billReminder = await _context.BillReminders
                                             .Include(u => u.User).FirstOrDefaultAsync(b => b.Id == id);
            
            if (billReminder == null)
                return null;

            return billReminder;
        }

        public async Task<BillReminder> Create(BillReminderDTO model)
        {
            var exists = _context.BillReminders.Any(b => b.Name == model.Name && 
                                                         b.UserId == _currentUser.LoggedInUser());
            
            if (exists)
                return null;     

            var create = new BillReminder
            {
                Name = model.Name,
                AmountDue = model.AmountDue,
                DueDate = model.DueDate,
                IsPaid = model.IsPaid,
                UserId = _currentUser.LoggedInUser()
            };

            await _context.BillReminders.AddAsync(create);

            //Send notification
            await SendNotification(create.Name, create.UserId);

            await _context.SaveChangesAsync();

            return create;
        }

        public async Task<BillReminder> Update(BillReminderDTO update, int? id)
        {
            var existingBillReminder = await _context.BillReminders.FindAsync(id);
            if (existingBillReminder == null)
                return null;

            var exists = _context.BillReminders.Any(b => b.Name == update.Name && 
                                                         b.Id != id && b.UserId == _currentUser.LoggedInUser());
           
            if (exists)
                return null;

            existingBillReminder.Name = update.Name;
            existingBillReminder.AmountDue = update.AmountDue;
            existingBillReminder.DueDate = update.DueDate;
            existingBillReminder.IsPaid = update.IsPaid;
            existingBillReminder.UserId = _currentUser.LoggedInUser();

            await _context.SaveChangesAsync();

            return existingBillReminder;
        }

        public async Task<BillReminder> Delete(int? id)
        {
            var billReminder = await _context.BillReminders.FindAsync(id);
            if (billReminder == null)
                return null;

            _context.BillReminders.Remove(billReminder);
            await _context.SaveChangesAsync();

            return billReminder;
        }

        private async Task<Notification> SendNotification(string name, int userId)
        {

            var notification = new Notification
            {
                NotificationTypeId = (int)NotificationTypeIds.Bill_Reminder,
                Message = name,
                Read = false,
                UserId = userId
            };

            await _context.Notifications.AddAsync(notification);

            return notification;
        }
    }
}
