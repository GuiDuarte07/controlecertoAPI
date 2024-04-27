using Finantech.Enums;

namespace Finantech.DTOs.Category
{
    public class InfoCategoryResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public BillTypeEnum BillType { get; set; }
    }
}
