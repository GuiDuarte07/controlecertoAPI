using System.ComponentModel.DataAnnotations;

namespace ControleCerto.DTOs.Invoice
{
    public class SimulateCreditPurchaseInvoiceRequest
    {
        [Required(ErrorMessage = "Campo 'CreditCardId' não informado.")]
        public long CreditCardId { get; set; }

        [Required(ErrorMessage = "Campo 'PurchaseDate' não informado.")]
        public DateTime PurchaseDate { get; set; }
    }
}
