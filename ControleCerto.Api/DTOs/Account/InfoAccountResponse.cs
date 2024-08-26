using ControleCerto.Enums;

namespace ControleCerto.DTOs.Account
{
    public class InfoAccountResponse
    {
        public long Id { get; set; }
        public double Balance { get; set; }
        public string? Description { get; set; }
        public string Bank { get; set; }
        public string Color { get; set; }
    }
}
