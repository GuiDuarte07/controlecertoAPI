using System.ComponentModel.DataAnnotations;

namespace Finantech.DTOs.CreditPurcchase
{
    public class CreateCreditPurchaseRequest
    {
        [Required(ErrorMessage = "Campo 'TotalAmount' não informado.")]
        public double TotalAmount { get; set; }

        [Required(ErrorMessage = "Campo 'TotalInstalment' não informado.")]
        public int TotalInstalment { get; set; }

        [Required(ErrorMessage = "Campo 'PurchaseDate' não informado.")]
        public DateTime? PurchaseDate { get; set; }

        [Required(ErrorMessage = "Campo 'Destination' não informado.")]
        [MaxLength(80, ErrorMessage = "Campo 'Destination' pode conter até 80 caracteres")]
        public string Destination { get; set; }

        [MaxLength(100, ErrorMessage = "Campo 'Description' pode conter até 100 caracteres")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Campo 'CreditCardId' não informado.")]
        public int CreditCardId { get; set; }
    }
}
