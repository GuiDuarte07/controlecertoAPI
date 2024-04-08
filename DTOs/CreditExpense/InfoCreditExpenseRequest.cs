namespace Finantech.DTOs.CreditCardExpense
{
    public class InfoCreditExpenseRequest
    {
        public double Amount { get; set; }
        public string? Description { get; set; }
        public DateTime PurchaseDate { get; set; }
        public int InstallmentNumber { get; set; }
        public string Destination { get; set; }
        public int CreditPurchaseId { get; set; }
    }
}
