namespace Finantech.Models.Entities
{
    public class Transference
    {
        public long Id { get; set; }
        public string Description { get; set; }
        public double Amount { get; set; }
        public string PurchaseDate { get; set; }
        public long AccountDestinyId { get; set; }
        public long AccountOriginId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }

        public Account AccountDestiny { get; set; }
        public Account AccountOrigin { get; set; }
    }
}
