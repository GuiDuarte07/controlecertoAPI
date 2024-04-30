using System.ComponentModel.DataAnnotations;

namespace Finantech.DTOs.CreditCard
{
    public class CreateCreditCardRequest
    {

        [Required(ErrorMessage = "Campo 'TotalLimit' não informado.")]
        public double TotalLimit { get; set; }

        [Required(ErrorMessage = "Campo 'UsedLimit' não informado.")]
        public double UsedLimit { get; set; }

        [MaxLength(100, ErrorMessage = "Campo 'Description' pode conter até 100 caracteres")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Campo 'CardBrand' não informado.")]
        public string CardBrand { get; set; }

        [Required(ErrorMessage = "Campo 'AccountId' não informado.")]
        public int AccountId { get; set; }

        [Required(ErrorMessage = "Campo 'DueDay' não informado.")]
        public int DueDay { get; set; }

        [Required(ErrorMessage = "Campo 'CloseDay' não informado.")]
        public int CloseDay { get; set; }
    }
}
