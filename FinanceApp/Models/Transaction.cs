using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FinanceApp.Models
{
    [Index(nameof(UserId), Name = "IX_Transaction_UserId")]
    public class Transaction
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Description is required!")]
        public string Description { get; set; } = null!;

        [Required(ErrorMessage = "Amount is required!")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount needs to be higher than 0")]
        public double Amount { get; set; }

        [Required(ErrorMessage = "Category is required!")]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        [Required(ErrorMessage = "Transaction Date is required!")]
        public DateTime TransactionDate { get; set; }

        [Required(ErrorMessage = "Payment Method is required!")]
        public int PaymentMethodId { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        public int UserId { get; set; }
        public User? User { get; set; }
    }
}
