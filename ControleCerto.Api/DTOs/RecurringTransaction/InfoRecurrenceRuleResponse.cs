using ControleCerto.Enums;

namespace ControleCerto.DTOs.RecurringTransaction
{
    public class InfoRecurrenceRuleResponse
    {
        public long Id { get; set; }
        public RecurrenceFrequencyEnum Frequency { get; set; }
        public bool IsEveryDay { get; set; }
        public string? DaysOfWeek { get; set; }
        public int? DayOfWeek { get; set; }
        public int? DayOfMonth { get; set; }
        public int? MonthOfYear { get; set; }
        public int? DayOfMonthForYearly { get; set; }
        public int Interval { get; set; }
    }
}
