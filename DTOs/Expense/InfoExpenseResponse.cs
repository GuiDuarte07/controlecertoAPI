using Finantech.Enums;

namespace Finantech.DTOs.Expense
{
    public class InfoExpenseResponse
    {
        public int Id { get; set; }
        public double Amount { get; set; }
        public ExpenseTypeEnum ExpenseType { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string Destination { get; set; }
        public string? Description { get; set; }
        public bool JustForRecord { get; set; } 
    }
}
