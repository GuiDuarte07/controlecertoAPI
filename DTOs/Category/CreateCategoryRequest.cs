using Finantech.Enums;
using System.ComponentModel.DataAnnotations;

namespace Finantech.DTOs.Category
{
    public class CreateCategoryRequest
    {
        [Required(ErrorMessage = "Campo 'Name' não informado.")]
        [MaxLength(60, ErrorMessage = "Campo 'Name' pode conter até 60 caracteres")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Campo 'Icon' não informado.")]
        public string Icon { get; set; }
        [Required(ErrorMessage = "Campo 'BillType' não informado.")]
        public BillTypeEnum BillType { get; set; }
        [Required(ErrorMessage = "Campo 'Color' não informado.")]
        public string Color { get; set; }
    }
}
