using System.ComponentModel.DataAnnotations;

namespace FinanceApp.DTOs
{
    public class CategoryDTO
    {
        [Required(ErrorMessage = "Name is required!")]
        public string Name { get; set; }
        public int UserId { get; set; }
    }
}
