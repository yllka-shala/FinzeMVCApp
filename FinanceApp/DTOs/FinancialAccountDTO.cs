using FinanceApp.Models;
using System.ComponentModel.DataAnnotations;

namespace FinanceApp.DTOs
{
    public class FinancialAccountDTO
    {
        [Required(ErrorMessage = "Account Type is required!")]
        public int AccountTypeId { get; set; }

        [Required(ErrorMessage = "Account Balance is required!")]
        public double AccountBalance { get; set; }

        public string? Bank { get; set; }
        public int UserId { get; set; }
    }
}
