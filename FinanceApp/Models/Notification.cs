using System.ComponentModel.DataAnnotations;

namespace FinanceApp.Models
{
    public class Notification
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Notification Type is required!")]
        public int NotificationTypeId { get; set; }
        public NotificationType? NotificationType { get; set; }

        [Required(ErrorMessage = "Message is required!")]
        public string Message { get; set; }

        public bool Read { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        public int UserId { get; set; }
        public User? User { get; set; }
    }
}
