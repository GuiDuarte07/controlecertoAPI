using System.ComponentModel.DataAnnotations;

namespace Finantech.DTOs.Invoice
{
    public class CreteInvoicePaymentRequest
    {
        [Range(0, double.MaxValue, ErrorMessage = "O 'AmountPaid' deve ser um número positivo.")]
        [Required(ErrorMessage = "Campo 'AmountPaid' não informado.")]
        public double AmountPaid { get; set; }

        [MaxLength(100, ErrorMessage = "Campo 'Description' pode conter até 100 caracteres")]
        [Required(ErrorMessage = "Campo 'Description' não informado.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Campo 'PaymentDate' não informado.")]
        public DateTime PaymentDate { get; set; }

        [Required(ErrorMessage = "Campo 'InvoiceId' não informado.")]
        public int InvoiceId { get; set; }

        public int? AccountPaidId { get; set; }

        public Boolean JustForRecord { get; set; } = false;
    }
}
