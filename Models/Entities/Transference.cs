namespace Finantech.Models.Entities
{
    public class Transference
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public double? Amount { get; set; }
        public string PurchaseDate { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
        public int AccountDestinyId { get; set; }
        public int AccountOriginId { get; set; }

        public Account AccountDestiny { get; set; }
        public Account AccountOrigin { get; set; }
    }
}
