using System.ComponentModel.DataAnnotations;

namespace ControleCerto.Modules.Mcp.DTOs
{
    public class CreateCreditPurchasePayload
    {
        [Range(1, long.MaxValue, ErrorMessage = "Campo 'creditCardId' deve ser maior que zero.")]
        public long CreditCardId { get; set; }

        [Required(ErrorMessage = "Campo 'totalAmount' não informado.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O campo 'totalAmount' deve ser positivo.")]
        public double TotalAmount { get; set; }

        [Required(ErrorMessage = "Campo 'totalInstallment' não informado.")]
        [Range(1, int.MaxValue, ErrorMessage = "O campo 'totalInstallment' deve ser ao menos 1.")]
        public int TotalInstallment { get; set; }

        [Required(ErrorMessage = "Campo 'installmentsPaid' não informado.")]
        [Range(0, int.MaxValue, ErrorMessage = "O campo 'installmentsPaid' deve ser um número inteiro válido.")]
        public int InstallmentsPaid { get; set; }

        [Required(ErrorMessage = "Campo 'purchaseDate' não informado.")]
        public DateTime PurchaseDate { get; set; }

        [Required(ErrorMessage = "Campo 'destination' não informado.")]
        [MaxLength(80, ErrorMessage = "Campo 'destination' pode conter até 80 caracteres.")]
        public string Destination { get; set; } = string.Empty;

        [MaxLength(100, ErrorMessage = "Campo 'description' pode conter até 100 caracteres.")]
        public string? Description { get; set; }

        [Range(1, long.MaxValue, ErrorMessage = "Campo 'categoryId' deve ser maior que zero.")]
        public long CategoryId { get; set; }
    }
}
