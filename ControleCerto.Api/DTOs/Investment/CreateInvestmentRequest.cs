namespace ControleCerto.DTOs.Investment
{
    public class CreateInvestmentRequest
    {
        public string Name { get; set; }
        public double? InitialAmount { get; set; }
        public DateTime? StartDate { get; set; }
        public string? Description { get; set; }
    }
}
