using Finantech.Enums;

namespace Finantech.DTOs.Income
{
    public class InfoIncomeResponse
    {
        public int Id { get; set; }
        public string? Description { get; set; }
        public double Amount { get; set; }
        public IncomeTypeEnum IncomeType { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string Origin { get; set; }
    }
}
