using System.ComponentModel.DataAnnotations;

namespace FinanceApp.DTOs
{
    public class BillReminderDTO
    {
        [Required(ErrorMessage = "Nae is required!")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Amount Due is required!")]
        public double AmountDue { get; set; }

        [Required(ErrorMessage = "Due Date is required!")]
        public DateTime DueDate { get; set; }

        public bool IsPaid { get; set; }
        public int UserId { get; set; }
    }
}
