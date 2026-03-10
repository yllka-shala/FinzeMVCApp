using System.ComponentModel.DataAnnotations;

namespace FinanceApp.DTOs
{
    public class GoalDTO
    {
        [Required(ErrorMessage = "Name is required!")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Target Amount is required!")]
        public double TargetAmount { get; set; }

        [Required(ErrorMessage = "Current Amount is required!")]
        public double CurrentAmount { get; set; }

        [Required(ErrorMessage = "Due Date is required!")]
        public DateTime DueDate { get; set; }
        public int UserId { get; set; }
    }
}
