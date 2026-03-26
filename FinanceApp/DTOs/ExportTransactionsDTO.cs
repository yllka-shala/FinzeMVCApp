using FinanceApp.Models;
using System.ComponentModel.DataAnnotations;

namespace FinanceApp.DTOs
{
    public class ExportTransactionsDTO
    {
        public int Id { get; set; }
        public string Description { get; set; } = null!;
        public string Amount { get; set; }
        public string Category { get; set; }
        public string TransactionDate { get; set; }
        public string PaymentMethod { get; set; }
        public string CreatedDate { get; set; } 
        public string User { get; set; }
    }
}
