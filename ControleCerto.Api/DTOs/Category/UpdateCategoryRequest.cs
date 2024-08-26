using System.ComponentModel.DataAnnotations;

namespace ControleCerto.DTOs.Category
{
    public class UpdateCategoryRequest
    {
        [Required(ErrorMessage = "Campo 'Id' não informado.")]
        public int Id { get; set; }
        [MaxLength(60, ErrorMessage = "Campo 'Name' pode conter até 60 caracteres")]
        public string? Name { get; set; }
        public string? Icon { get; set; }
        public string? Color { get; set; }
    }
}
