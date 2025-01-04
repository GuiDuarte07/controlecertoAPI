namespace ControleCerto.DTOs.Category
{
    public class InfoLimitResponse
    {
        public bool IsParentLimit { get; set; }
        public double ActualLimit { get; set; }
        public double AvailableMonthLimit { get; set; }
        public double AccumulatedLimit { get; set; }
    }
}
