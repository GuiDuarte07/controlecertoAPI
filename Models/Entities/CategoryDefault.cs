using Finantech.Enums;

namespace Finantech.Models.Entities
{
    public class CategoryDefault
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }
        public BillTypeEnum BillType { get; set; }
    }
}
