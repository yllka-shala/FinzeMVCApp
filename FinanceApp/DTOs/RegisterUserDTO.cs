using System.ComponentModel.DataAnnotations;

namespace FinanceApp.DTOs
{
    public class RegisterUserDTO
    {
        [Required(ErrorMessage = "FirstName is required!")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "LastName is required!")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required!")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required!")]
        [StringLength(100, ErrorMessage = "Must be between 8 and 100 characters", MinimumLength = 8)]
        public string Password { get; set; }
        public string? ProfilePicture { get; set; }
    }
}
