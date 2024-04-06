using System.ComponentModel.DataAnnotations;

namespace Finantech.DTOs.CreditCard
{
    public class CreateCreditCardRequest
    {
        [Required(ErrorMessage = "Campo 'Number' não informado.")]
        public string Number { get; set; }

        [Required(ErrorMessage = "Campo 'Limit' não informado.")]
        public double? Limit { get; set; }

        [MaxLength(100, ErrorMessage = "Campo 'Description' pode conter até 100 caracteres")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Campo 'CardBrand' não informado.")]
        public string CardBrand { get; set; }

        [Required(ErrorMessage = "Campo 'CreditType' não informado.")]
        public string CreditType { get; set; }
    }
}
