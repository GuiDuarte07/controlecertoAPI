using ControleCerto.Enums;
using System.ComponentModel.DataAnnotations;

namespace ControleCerto.DTOs.RecurringTransaction
{
    public class CreateRecurrenceRuleRequest
    {
        [Required(ErrorMessage = "Frequência é obrigatória")]
        public RecurrenceFrequencyEnum Frequency { get; set; }

        public bool IsEveryDay { get; set; } = true;

        [StringLength(50, ErrorMessage = "Dias da semana devem ter no máximo 50 caracteres")]
        public string? DaysOfWeek { get; set; }

        [Range(1, 7, ErrorMessage = "Dia da semana deve estar entre 1 e 7")]
        public int? DayOfWeek { get; set; }

        [Range(-1, 31, ErrorMessage = "Dia do mês deve estar entre 1-31 ou -1 para último dia")]
        public int? DayOfMonth { get; set; }

        [Range(1, 12, ErrorMessage = "Mês deve estar entre 1 e 12")]
        public int? MonthOfYear { get; set; }

        [Range(1, 31, ErrorMessage = "Dia do mês para recorrência anual deve estar entre 1 e 31")]
        public int? DayOfMonthForYearly { get; set; }

        [Required(ErrorMessage = "Intervalo é obrigatório")]
        [Range(1, int.MaxValue, ErrorMessage = "Intervalo deve ser maior que zero")]
        public int Interval { get; set; } = 1;
    }
}
