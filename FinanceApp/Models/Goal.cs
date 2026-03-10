using System.ComponentModel.DataAnnotations;

namespace FinanceApp.Models
{
    public class Goal
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required!")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Target Amount is required!")]
        public double TargetAmount { get; set; }

        [Required(ErrorMessage = "Current Amount is required!")]
        public double CurrentAmount { get; set; }

        [Required(ErrorMessage = "Due Date is required!")]
        public DateTime DueDate { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public int UserId { get; set; }
        public User? User { get; set; }
        public double Progress => CurrentAmount / TargetAmount * 100;
    }
}
