using ControleCerto.Enums;

namespace ControleCerto.DTOs.Category
{
    public class InfoParentCategoryResponse
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public BillTypeEnum BillType { get; set; }
        public string Color { get; set; }
        public double Amount { get; set; }
        public double? Limit { get; set; }

        public IEnumerable<InfoCategoryResponse> SubCategories { get; set; }
    }
}
