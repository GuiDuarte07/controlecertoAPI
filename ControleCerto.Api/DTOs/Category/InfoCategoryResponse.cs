using ControleCerto.Enums;

namespace ControleCerto.DTOs.Category
{
    public class InfoCategoryResponse
    {
        public long Id { get; set; }
        public long? parentId { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public BillTypeEnum BillType { get; set; }
        public string Color { get; set; }
        public double? Limit { get; set; }
    }
}
