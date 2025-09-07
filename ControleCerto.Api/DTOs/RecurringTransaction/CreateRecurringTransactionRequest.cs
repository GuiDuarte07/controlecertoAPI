using ControleCerto.Enums;
using System.ComponentModel.DataAnnotations;

namespace ControleCerto.DTOs.RecurringTransaction
{
    public class CreateRecurringTransactionRequest
    {
        [Required(ErrorMessage = "Descrição é obrigatória")]
        [StringLength(200, ErrorMessage = "Descrição deve ter no máximo 200 caracteres")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Valor é obrigatório")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Valor deve ser maior que zero")]
        public double Amount { get; set; }

        [Required(ErrorMessage = "Tipo de transação é obrigatório")]
        public TransactionTypeEnum Type { get; set; }

        [Required(ErrorMessage = "Data de início é obrigatória")]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Required(ErrorMessage = "ID da conta é obrigatório")]
        public long AccountId { get; set; }

        [Required(ErrorMessage = "ID da categoria é obrigatório")]
        public long CategoryId { get; set; }

        [Required(ErrorMessage = "Regra de recorrência é obrigatória")]
        public CreateRecurrenceRuleRequest RecurrenceRule { get; set; }
    }
}
