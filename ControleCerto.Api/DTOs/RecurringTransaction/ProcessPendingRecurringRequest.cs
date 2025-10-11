using ControleCerto.Enums;
using System.ComponentModel.DataAnnotations;

namespace ControleCerto.DTOs.RecurringTransaction
{
    public class ProcessPendingRecurringRequest
    {
        public List<long> PendingTransactions { get; set; } = new List<long>();

        [Required(ErrorMessage = "Ação é obrigatória")]
        public InstanceStatusEnum Action { get; set; }

        [StringLength(300, ErrorMessage = "Motivo Rejeição deve ter no máximo 300 caracteres")]
        public string? RejectReason { get; set; }
    }
}
