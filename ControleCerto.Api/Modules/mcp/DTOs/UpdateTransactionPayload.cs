using System.ComponentModel.DataAnnotations;

namespace ControleCerto.Modules.Mcp.DTOs
{
    public class UpdateTransactionPayload
    {
        [Range(1, long.MaxValue, ErrorMessage = "Campo 'transactionId' deve ser maior que zero.")]
        public long TransactionId { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "O campo 'amount' deve ser positivo.")]
        public double? Amount { get; set; }

        public DateTime? PurchaseDate { get; set; }

        [MaxLength(80, ErrorMessage = "Campo 'destination' pode conter até 80 caracteres.")]
        public string? Destination { get; set; }

        [MaxLength(100, ErrorMessage = "Campo 'description' pode conter até 100 caracteres.")]
        public string? Description { get; set; }

        [MaxLength(300, ErrorMessage = "Campo 'observations' pode conter até 300 caracteres.")]
        public string? Observations { get; set; }

        public bool? JustForRecord { get; set; }
        [Range(1, long.MaxValue, ErrorMessage = "Campo 'categoryId' deve ser maior que zero.")]
        public long? CategoryId { get; set; }
    }
}
