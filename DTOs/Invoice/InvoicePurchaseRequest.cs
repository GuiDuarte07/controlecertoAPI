using System.ComponentModel.DataAnnotations;

namespace Finantech.DTOs.Invoice
{
    public class InvoicePurchaseRequest
    {
        [Required(ErrorMessage = "Campo 'AmountPaid' não informado.")]
        public double AmountPaid { get; set; }

        public string Description { get; set; }

        [Required(ErrorMessage = "Campo 'DateTime' não informado.")]
        public DateTime PaymentDate { get; set; }
    }
}
