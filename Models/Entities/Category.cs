using Finantech.Enums;

namespace Finantech.Models.Entities
{
    public class Category
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }
        public BillTypeEnum BillType { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; } = DateTime.Now;
        public int UserId { get; set; }

        public User User { get; set; }
        public ICollection<Transaction> Transactions { get; set; }

        public Category() { }

        public Category(CategoryDefault categoryDefault, int userId)
        {
            Name = categoryDefault.Name;
            Icon = categoryDefault.Icon;
            Color = categoryDefault.Color;
            BillType = categoryDefault.BillType;
            UserId = userId;
        }
    }

}
