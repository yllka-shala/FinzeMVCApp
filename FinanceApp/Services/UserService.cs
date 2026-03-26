using ClosedXML.Excel;
using FinanceApp.Data;
using FinanceApp.DTOs;
using FinanceApp.Helpers;
using FinanceApp.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto.Generators;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
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
            var exportData = await GetUserExportData();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Users");

            worksheet.Cell("A1").InsertTable(exportData);

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> ExportPdf()
        {
            var exportData = await GetUserExportData();

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

                    page.Header().Text("Users Report").FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);

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
                        });

                        // Add Header
                        table.Header(header =>
                        {
                            header.Cell().BorderBottom(2).Padding(3).Text("Id").Bold();
                            header.Cell().BorderBottom(2).Padding(3).Text("Name").Bold();
                            header.Cell().BorderBottom(2).Padding(3).Text("Email").Bold();
                            header.Cell().BorderBottom(2).Padding(3).Text("Role").Bold();
                            header.Cell().BorderBottom(2).Padding(3).Text("IsActive").Bold();
                            header.Cell().BorderBottom(2).Padding(3).Text("CreatedDate").Bold();
                        });

                        // Add Data Rows
                        foreach (var item in exportData)
                        {
                            table.Cell().Padding(3).Text(item.Id.ToString());
                            table.Cell().Padding(3).Text(item.Name);
                            table.Cell().Padding(3).Text(item.Email);
                            table.Cell().Padding(3).Text(item.Role);
                            table.Cell().Padding(3).Text(item.IsActive);
                            table.Cell().Padding(3).Text(item.CreatedDate);
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

        private async Task<List<ExportUserDTO>> GetUserExportData()
        {
            var query = _context.Users
                                .Include(u => u.Role)
                                .AsQueryable();

            var user = await query.ToListAsync();

            return user.Select(u => new ExportUserDTO
            {
                Id = u.Id,
                Name = $"{u.FirstName} {u.LastName}",
                Email = u.Email,
                Role = u.Role!.Name,
                IsActive = u.IsActive.ToString(),
                CreatedDate = u.CreatedDate.ToString("dd-MM-yyyy")
            }).ToList();
        }
    }
}
