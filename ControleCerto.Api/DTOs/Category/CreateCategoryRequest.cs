using ControleCerto.Enums;
using System.ComponentModel.DataAnnotations;

namespace ControleCerto.DTOs.Category
{
    public class CreateCategoryRequest
    {
        public long? parentId { get; set; }

        [Required(ErrorMessage = "Campo 'Name' não informado.")]
        [MaxLength(60, ErrorMessage = "Campo 'Name' pode conter até 60 caracteres")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Campo 'Icon' não informado.")]
        public string Icon { get; set; }
        [Required(ErrorMessage = "Campo 'BillType' não informado.")]
        public BillTypeEnum BillType { get; set; }
        [Required(ErrorMessage = "Campo 'Color' não informado.")]
        public string Color { get; set; }
        public double? Limit { get; set; }

    }
}
