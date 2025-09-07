using System.ComponentModel.DataAnnotations;

namespace ControleCerto.DTOs.RecurringTransaction
{
    public class ConfirmMultipleInstancesRequest
    {
        [Required(ErrorMessage = "IDs das instâncias são obrigatórios")]
        [MinLength(1, ErrorMessage = "Deve selecionar pelo menos uma instância")]
        public List<long> InstanceIds { get; set; }

        [Required(ErrorMessage = "Ação é obrigatória")]
        public InstanceActionEnum Action { get; set; }

        [StringLength(500, ErrorMessage = "Motivo da rejeição deve ter no máximo 500 caracteres")]
        public string? RejectionReason { get; set; }
    }
}
