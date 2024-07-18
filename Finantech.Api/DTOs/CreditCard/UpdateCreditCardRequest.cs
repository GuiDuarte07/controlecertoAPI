using System.ComponentModel.DataAnnotations;

namespace Finantech.DTOs.CreditCard
{
    public class UpdateCreditCardRequest
    {
        [Required(ErrorMessage = "Campo 'Id' não informado.")]
        public int Id { get; set; }

        public double? TotalLimit { get; set; }

        [MaxLength(100, ErrorMessage = "Campo 'Description' pode conter até 100 caracteres")]
        public string? Description { get; set; }

        /*public int? DueDay { get; set; }

        public int? CloseDay { get; set; }*/
    }
}
