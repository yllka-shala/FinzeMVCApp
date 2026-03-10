using System.ComponentModel.DataAnnotations;

namespace FinanceApp.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required!")]
        public string Name { get; set; }
        public int UserId { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
    }
}
