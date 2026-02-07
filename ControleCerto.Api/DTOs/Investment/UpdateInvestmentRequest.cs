namespace ControleCerto.DTOs.Investment
{
    public class UpdateInvestmentRequest
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public DateTime? StartDate { get; set; }
        public string? Description { get; set; }
    }
}
