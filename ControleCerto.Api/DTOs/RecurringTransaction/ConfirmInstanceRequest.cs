using System.ComponentModel.DataAnnotations;

namespace ControleCerto.DTOs.RecurringTransaction
{
    public class ConfirmInstanceRequest
    {
        [Required(ErrorMessage = "ID da instância é obrigatório")]
        public long InstanceId { get; set; }

        [Required(ErrorMessage = "Ação é obrigatória")]
        public InstanceActionEnum Action { get; set; }

        [StringLength(500, ErrorMessage = "Motivo da rejeição deve ter no máximo 500 caracteres")]
        public string? RejectionReason { get; set; }
    }

    public enum InstanceActionEnum
    {
        CONFIRM,
        REJECT,
        SKIP
    }
}
