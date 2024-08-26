using System.ComponentModel.DataAnnotations;

namespace ControleCerto.DTOs.CreditPurchase
{
    public class CreateCreditPurchaseRequest
    {
        [Range(0, double.MaxValue, ErrorMessage = "O 'TotalAmount' deve ser um número positivo.")]
        [Required(ErrorMessage = "Campo 'TotalAmount' não informado.")]
        public double TotalAmount { get; set; }

        [Required(ErrorMessage = "Campo 'TotallInstallment' não informado.")]
        public int TotalInstallment { get; set; }

        [Required(ErrorMessage = "Campo 'InstallmentsPaid' não informado.")]
        public int InstallmentsPaid { get; set; } = 0;

        [Required(ErrorMessage = "Campo 'PurchaseDate' não informado.")]
        public DateTime? PurchaseDate { get; set; }

        [Required(ErrorMessage = "Campo 'Destination' não informado.")]
        [MaxLength(80, ErrorMessage = "Campo 'Destination' pode conter até 80 caracteres")]
        public string Destination { get; set; }

        [MaxLength(100, ErrorMessage = "Campo 'Description' pode conter até 100 caracteres")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Campo 'CreditCardId' não informado.")]
        public long CreditCardId { get; set; }

        [Required(ErrorMessage = "Campo 'CategoryId' não informado.")]
        public long CategoryId { get; set; }
    }
}
