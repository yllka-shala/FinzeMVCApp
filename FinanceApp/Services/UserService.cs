using ClosedXML.Excel;
using FinanceApp.Data;
using FinanceApp.DTOs;
using FinanceApp.Helpers;
using FinanceApp.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto.Generators;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FinanceApp.Services
{
    public class UserService : IUserService
    {
        private readonly FinanceAppContext _context;
        private readonly IFileService _fileService;

        public UserService(FinanceAppContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        public async Task<PaginatedList<User>> GetAll(string? search, int pageNumber = 1, int pageSize = 10)
        {
            var query = _context.Users
                                    .Include(u => u.Role)
                                    .AsNoTracking();

            if (query == null)
                return null;


            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p =>
                    p.FirstName.Contains(search) ||
                    p.LastName.Contains(search) ||
                    p.Email.Contains($"{search}") ||
                    p.Role.Name.Contains(search));
            }

            return await PaginatedList<User>.CreateAsync(query, pageNumber, pageSize);
        }

        public async Task<User> GetUserById(int? id)
        {
            var user = await _context.Users.Include(r => r.Role).FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return null;
            return user;
        }

        public async Task<User> UpdateUser(UpdateUserDTO update, int? id)
        {
            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
                return null;

            var exist = await _context.Users.AnyAsync(c => c.Email.ToLower() == update.Email.ToLower() && c.Id != update.Id);
            if (exist)
                return null;

            existingUser.FirstName = update.FirstName;
            existingUser.LastName = update.LastName;
            existingUser.Email = update.Email;
            existingUser.RoleId = update.RoleId;
            existingUser.IsActive = update.IsActive;

            await _context.SaveChangesAsync();
            return existingUser;
        }

        public async Task<User> DeleteUser(int? id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return null;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<User> ChangePasswordAsync(ChangePasswordDTO changePasswordDto)
        {
            var user = await _context.Users.FindAsync(changePasswordDto.UserId);
            if (user == null)
                return null;

            bool isCurrentPasswordValid = BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.Password);
            if (!isCurrentPasswordValid)
                return null;

            user.Password = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<List<Role>> GetAllRoles()
        {
            var roles = await _context.Roles.ToListAsync();
            if (roles == null)
                return null;

            return roles;
        }

        public async Task<User> UpdateProfile(UserProfileDTO profile, int? id)
        {
            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
                return null;

            var exist = await _context.Users.AnyAsync(c => c.Email.ToLower() == profile.Email.ToLower() && c.Id != profile.Id);
            if (exist)
                return null;

            if (profile.ImageFile != null)
            {
                profile.ProfilePicture = await _fileService.SaveFile(profile.ImageFile);

                //Delete the old picture from folder
                if(existingUser.ProfilePicture != null)
                {
                    _fileService.DeleteFile(existingUser.ProfilePicture);
                }
                
                existingUser.ProfilePicture = profile.ProfilePicture;
            }

            existingUser.FirstName = profile.FirstName;
            existingUser.LastName = profile.LastName;
            existingUser.Email = profile.Email;
            
            await _context.SaveChangesAsync();
            return existingUser;
        }

        public async Task<User> DeleteProfilePicture(int id)
        {
            if (id == 0)
                return null;

            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
                return null;

            if (string.IsNullOrEmpty(existingUser.ProfilePicture))
                return null;

            _fileService.DeleteFile(existingUser.ProfilePicture);
            existingUser.ProfilePicture = null;
            await _context.SaveChangesAsync();

            return existingUser;
        }

        public async Task<byte[]> ExportExcel()
        {
            var users = await _context.Users
                                    .Include(u => u.Role)
                                    .ToListAsync();

            var exportData = users.Select(u => new
            {
                Id = u.Id,
                User = $"{u.FirstName} {u.LastName}",
                Email = u.Email,
                Role = u.Role!.Name,
                IsActive = u.IsActive,
                CreatedDate = u.CreatedDate.ToString("dd-MM-yyyy")                
            }).ToList();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Users");

            worksheet.Cell("A1").InsertTable(exportData);

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}
