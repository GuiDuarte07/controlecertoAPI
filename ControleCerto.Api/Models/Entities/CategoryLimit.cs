namespace ControleCerto.Models.Entities
{
    public class CategoryLimit
    {
        public long Id { get; set; }
        public double Amount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; } = null;
        public long CategoryId { get; set; }
        public Category Category { get; set; }
    }
}
