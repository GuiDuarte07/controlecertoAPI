using ControleCerto.Enums;
using System.ComponentModel.DataAnnotations;

namespace ControleCerto.DTOs.Notification
{
    public class CreatePublicNotificationRequest
    {
        [Required(ErrorMessage = "Campo 'Title' não informado.")]
        [MaxLength(100, ErrorMessage = "Campo 'Title' pode conter até 100 caracteres")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Campo 'Message' não informado.")]
        [MaxLength(600, ErrorMessage = "Campo 'Message' pode conter até 600 caracteres")]
        public string Message { get; set; }

        public NotificationTypeEnum Type { get; set; }

        [MaxLength(100, ErrorMessage = "Campo 'ActionPath' pode conter até 100 caracteres")]
        public string? ActionPath { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}
