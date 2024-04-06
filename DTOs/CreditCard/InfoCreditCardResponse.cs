namespace Finantech.DTOs.CreditCard
{
    public class InfoCreditCardResponse
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public double Limit { get; set; }
        public string Description { get; set; }
        public string CardBrand { get; set; }
        public string CreditType { get; set; }
    }
}
