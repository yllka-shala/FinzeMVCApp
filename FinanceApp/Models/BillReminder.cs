using System.ComponentModel.DataAnnotations;

namespace FinanceApp.Models
{
    public class BillReminder
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required!")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Amount Due is required!")]
        public double AmountDue { get; set; }

        [Required(ErrorMessage = "Due Date is required!")]
        public DateTime DueDate { get; set; }
        
        public bool IsPaid { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public int UserId { get; set; }
        public User? User { get; set; }
    }
}
