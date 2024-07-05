using System.ComponentModel.DataAnnotations;

namespace Finantech.DTOs.CreditPurchase
{
    public class UpdateCreditPurchaseResponse
    {
        [Required(ErrorMessage = "Campo 'Id' não informado.")]
        public int Id { get; set; }

        public int TotalInstallment { get; set; }

        public DateTime? PurchaseDate { get; set; }

        [MaxLength(80, ErrorMessage = "Campo 'Destination' pode conter até 80 caracteres")]
        public string Destination { get; set; }

        [MaxLength(100, ErrorMessage = "Campo 'Description' pode conter até 100 caracteres")]
        public string? Description { get; set; }

        public int CreditCardId { get; set; }
    }
}
