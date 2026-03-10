using FinanceApp.Models;
using System.ComponentModel.DataAnnotations;

namespace FinanceApp.DTOs
{
    public class UpdateUserDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "FirstName is required!")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "LastName is required!")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required!")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; }
        public int RoleId { get; set; }
        public Role? Role { get; set; }
        public bool IsActive { get; set; }
    }
}
