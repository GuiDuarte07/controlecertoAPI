using Finantech.Enums;
using System.ComponentModel.DataAnnotations;

namespace Finantech.DTOs.Account
{
    public class CreateAccountRequest
    {
        [Required(ErrorMessage = "Campo 'Balance' não informado.")]
        public double Balance { get; set; }

        [MaxLength(100, ErrorMessage = "Campo 'Description' pode conter até 100 caracteres")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Campo 'Bank' não informado.")]
        public string Bank { get; set; }

        [Required(ErrorMessage = "Campo 'AccountType' não informado.")]
        public AccountTypeEnum AccountType { get; set; }

        public string Color { get; set; }

        [Required(ErrorMessage = "Campo 'UserId' não informado.")]
        public int UserId { get; set; }
    }
}
