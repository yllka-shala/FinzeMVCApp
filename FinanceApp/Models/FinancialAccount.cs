using System.ComponentModel.DataAnnotations;

namespace FinanceApp.Models
{
    public class FinancialAccount
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Account Type is required!")]
        public int AccountTypeId { get; set; }
        public AccountType? AccountType { get; set; }

        [Required(ErrorMessage = "Account Balance is required!")]
        public double AccountBalance { get; set; }

        public string? Bank { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        public int UserId { get; set; }
        public User? User { get; set; }
    }
}
