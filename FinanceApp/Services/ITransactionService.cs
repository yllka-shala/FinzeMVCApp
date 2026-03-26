using FinanceApp.DTOs;
using FinanceApp.Helpers;
using FinanceApp.Models;

namespace FinanceApp.Services
{
    public interface ITransactionService
    {
        Task<PaginatedList<Transaction>> GetAll(string? search, int pageNumber = 1, int pageSize = 10);
        Task<Transaction> GetById(int? id);
        Task<Transaction> Create(TransactionDTO create);
        Task<Transaction> Update(TransactionDTO update, int? id);
        Task<Transaction> Delete(int? id);
        Task<List<PaymentMethod>> GetAllPaymentMethods();
        Task<byte[]> ExportExcel();
        Task<byte[]> ExportPdf();
    }
}
