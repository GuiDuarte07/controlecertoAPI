using ControleCerto.Enums;
using System.ComponentModel.DataAnnotations;

namespace ControleCerto.DTOs.Account
{
    public class CreateAccountRequest
    {
        [Required(ErrorMessage = "Campo 'Balance' não informado.")]
        public double Balance { get; set; }

        [MaxLength(100, ErrorMessage = "Campo 'Description' pode conter até 100 caracteres")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Campo 'Bank' não informado.")]
        public string Bank { get; set; }
        public string Color { get; set; }
    }
}
