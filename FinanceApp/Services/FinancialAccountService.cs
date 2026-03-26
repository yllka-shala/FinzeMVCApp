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
    public class FinancialAccountService : IFinancialAccountService
    {
        private readonly FinanceAppContext _context;
        private readonly CurrentUser _currentUser;

        public FinancialAccountService(FinanceAppContext context, CurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<PaginatedList<FinancialAccount>> GetAll(string? search, int pageNumber = 1, int pageSize = 10)
        {
            var query = _context.FinancialAccounts
                                                .Include(fa => fa.AccountType)
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
                    p.AccountType.Name.Contains(search) ||
                    p.Bank.Contains(search) ||
                    p.User.FirstName.Contains(search) ||
                    p.User.LastName.Contains(search));
            }

            return await PaginatedList<FinancialAccount>.CreateAsync(query, pageNumber, pageSize);
        }

        public async Task<FinancialAccount> GetById(int? id)
        {
            var financialAccount = await _context.FinancialAccounts
                                                 .Include(fa => fa.AccountType)
                                                 .Include(u => u.User).FirstOrDefaultAsync(fa => fa.Id == id);
            
            if (financialAccount == null)
                return null;

            return financialAccount;
        }

        public async Task<FinancialAccount> Create(FinancialAccountDTO model)
        {
            var create = new FinancialAccount
            {
                AccountTypeId = model.AccountTypeId,
                AccountBalance = model.AccountBalance,
                Bank = model.Bank,
                UserId = _currentUser.LoggedInUser()
            };

            await _context.FinancialAccounts.AddAsync(create);
            await _context.SaveChangesAsync();

            return create;
        }

        public async Task<FinancialAccount> Update(FinancialAccountDTO update, int? id)
        {
            var existingFinancialAccounts = await _context.FinancialAccounts.FindAsync(id);
            if (existingFinancialAccounts == null)
                return null;

            existingFinancialAccounts.AccountTypeId = update.AccountTypeId;
            existingFinancialAccounts.AccountBalance = update.AccountBalance;
            existingFinancialAccounts.Bank = update.Bank;
            existingFinancialAccounts.UserId = _currentUser.LoggedInUser();

            await _context.SaveChangesAsync();

            return existingFinancialAccounts;
        }

        public async Task<FinancialAccount> Delete(int? id)
        {
            var financialAccount = await _context.FinancialAccounts.FindAsync(id);
            if (financialAccount == null)
                return null;

            _context.FinancialAccounts.Remove(financialAccount);
            await _context.SaveChangesAsync();

            return financialAccount;
        }

        public async Task<List<AccountType>> GetAccountTypeList()
        {
            var accountType = await _context.AccountTypes.ToListAsync();
            if (accountType == null)
                return null;

            return accountType;
        }

        public async Task<byte[]> ExportExcel()
        {
            var exportData = await GetFinancialAccountExportData();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("FinancialAccount");

            worksheet.Cell("A1").InsertTable(exportData);

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> ExportPdf()
        {
            var exportData = await GetFinancialAccountExportData();

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

                    page.Header().Text("Financial Account Report").FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);

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
                            header.Cell().BorderBottom(2).Padding(3).Text("AccountType").Bold();
                            header.Cell().BorderBottom(2).Padding(3).Text("AccountBalance").Bold();
                            header.Cell().BorderBottom(2).Padding(3).Text("Bank").Bold();
                            header.Cell().BorderBottom(2).Padding(3).Text("CreatedDate").Bold();
                            header.Cell().BorderBottom(2).Padding(3).Text("User").Bold();
                        });

                        // Add Data Rows
                        foreach (var item in exportData)
                        {
                            table.Cell().Padding(3).Text(item.Id.ToString());
                            table.Cell().Padding(3).Text(item.AccountType);
                            table.Cell().Padding(3).Text(item.AccountBalance);
                            table.Cell().Padding(3).Text(item.Bank);
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

        private async Task<List<ExportFinancialAccountDataDTO>> GetFinancialAccountExportData()
        {
            var query = _context.FinancialAccounts
                                                .Include(fa => fa.AccountType)
                                                .Include(u => u.User)
                                                .AsQueryable();


            if (!_currentUser.IsAdmin())
            {
                var userId = _currentUser.LoggedInUser();
                query = query.Where(t => t.UserId == userId);
            }

            var financialAccounts = await query.ToListAsync();

            return financialAccounts.Select(u => new ExportFinancialAccountDataDTO
            {
                Id = u.Id,
                AccountType = u.AccountType!.Name,
                AccountBalance = $"{u.AccountBalance}$",
                Bank = u.Bank,
                CreatedDate = u.CreatedDate.ToString("dd-MM-yyyy"),
                User = $"{u.User!.FirstName} {u.User.LastName}"
            }).ToList();
        }
    }
}
