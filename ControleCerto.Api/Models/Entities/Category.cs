using ControleCerto.Enums;
using System.Reflection.Metadata;

namespace ControleCerto.Models.Entities
{
    public class Category
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }
        public BillTypeEnum BillType { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;

        public long? ParentId { get; set; }
        public Category? Parent { get; set; }

        public int UserId { get; set; }

        public User User { get; set; }
        public ICollection<Transaction> Transactions { get; set; }

        public Category() { }

        public Category(string name, string icon, string color, BillTypeEnum billType, int userId)
        {
            Name = name;
            Icon = icon;
            Color = color;
            BillType = billType;
            UserId = userId;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }

}
