using System.ComponentModel.DataAnnotations;

namespace FinanceApp.Models
{
    public class Recurring
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required!")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Amount is required!")]
        public double Amount { get; set; }

        [Required(ErrorMessage = "Due Date is required!")]
        public DateTime DueDate { get; set; }

        [Required(ErrorMessage = "Frequency is required!")]
        public int FrequencyId { get; set; }
        public Frequency? Frequency { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public int UserId { get; set; }
        public User? User { get; set; }
    }
}
