using System.ComponentModel.DataAnnotations;

namespace FinanceApp.Models
{
    public class Budget
    {
        public int Id { get; set; }

        [Required(ErrorMessage= "Category is required!")]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        [Required(ErrorMessage = "Limit is required!")]
        public double Limit { get; set; }

        public double? CurrentSpent { get; set; }
        public double? Remaining { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        public int UserId { get; set; }
        public User? User { get; set; }
    }
}
