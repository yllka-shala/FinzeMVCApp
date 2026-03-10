using System.ComponentModel.DataAnnotations;

namespace FinanceApp.DTOs
{
    public class RecurringDTO
    {
        [Required(ErrorMessage = "Name is required!")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Amount is required!")]
        public double Amount { get; set; }

        [Required(ErrorMessage = "Due Date is required!")]
        public DateTime DueDate { get; set; }

        [Required(ErrorMessage = "Frequency is required!")]
        public int FrequencyId { get; set; }
        public int UserId { get; set; }
    }
}
