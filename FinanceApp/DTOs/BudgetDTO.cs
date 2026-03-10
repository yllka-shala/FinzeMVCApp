using FinanceApp.Models;
using System.ComponentModel.DataAnnotations;

namespace FinanceApp.DTOs
{
    public class BudgetDTO
    {
        [Required(ErrorMessage = "Category is required!")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Limit is required!")]
        public double Limit { get; set; }

        public double? CurrentSpent { get; set; }
        public double? Remaining { get; set; }
        public int UserId { get; set; }
    }
}
