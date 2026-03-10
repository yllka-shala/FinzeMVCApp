using System.ComponentModel.DataAnnotations;

namespace FinanceApp.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "FirstName is required!")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "LastName is required!")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required!")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required!")]
        [StringLength(100, ErrorMessage = "Must be between 8 and 100 characters", MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@#$!%*?&])[A-Za-z\d@#$!%*?&]{8,}$",
            ErrorMessage = "Password must include uppercase, lowercase, number, and special character.")]
        public string Password { get; set; }

        public string? ProfilePicture { get; set; }
        public int RoleId { get; set; }
        public Role? Role { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
        public string? VerificationToken { get; set; }
        public DateTime? Verified { get; set; }
        public bool IsVerified => Verified.HasValue;
        public DateTime EmailTokenExpires { get; set; }
        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpires { get; set; }
        public DateTime? PasswordReset { get; set; }

    }
}
