using System.ComponentModel.DataAnnotations;

namespace FinanceApp.DTOs
{
    public class TransactionDTO
    {
        [Required(ErrorMessage = "Description is required!")]
        public string Description { get; set; } = null!;

        [Required(ErrorMessage = "Amount is required!")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount needs to be higher than 0")]
        public double Amount { get; set; }

        [Required(ErrorMessage = "Category is required!")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Transaction Date is required!")]
        public DateTime TransactionDate { get; set; }

        [Required(ErrorMessage = "Payment Method is required!")]
        public int PaymentMethodId { get; set; }
        public int UserId { get; set; }
    }
}
