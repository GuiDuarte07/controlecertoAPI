namespace Finantech.Models.Entities
{
    public class Transference
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public double Amount { get; set; }
        public string PurchaseDate { get; set; }
        public int AccountDestinyId { get; set; }
        public int AccountOriginId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; }

        public Account AccountDestiny { get; set; }
        public Account AccountOrigin { get; set; }
    }
}
