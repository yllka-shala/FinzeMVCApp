using FinanceApp.Models;
using System.ComponentModel.DataAnnotations;

namespace FinanceApp.DTOs
{
    public class NotificationDTO
    {
        [Required(ErrorMessage = "Notification Type is required!")]
        public int NotificationTypeId { get; set; }

        [Required(ErrorMessage = "Message is required!")]
        public string Message { get; set; }

        public bool Read { get; set; }
        public int UserId { get; set; }
    }
}
