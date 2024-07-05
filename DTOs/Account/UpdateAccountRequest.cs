using Finantech.Enums;
using System.ComponentModel.DataAnnotations;

namespace Finantech.DTOs.Account
{
    public class UpdateAccountRequest
    {
        [Required(ErrorMessage = "Campo 'Id' não informado.")]
        public int Id { get; set; }

        [MaxLength(100, ErrorMessage = "Campo 'Description' pode conter até 100 caracteres")]
        public string? Description { get; set; }

        public string? Bank { get; set; }

        public string? Color { get; set; }
    }
}
