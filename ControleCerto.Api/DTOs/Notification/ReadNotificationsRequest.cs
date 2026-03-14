using System.ComponentModel.DataAnnotations;

namespace ControleCerto.DTOs.Notification
{
    public class ReadNotificationsRequest
    {
        [Required(ErrorMessage = "Campo 'NotificationIds' não informado.")]
        [MinLength(1, ErrorMessage = "Informe ao menos um ID para marcação de leitura.")]
        public ICollection<long> NotificationIds { get; set; } = new List<long>();
    }
}
