using Finantech.Enums;
using System.ComponentModel.DataAnnotations;

namespace Finantech.DTOs.Transaction
{
    public class UpdateTransactionRequest
    {
        [Required(ErrorMessage = "Campo 'Id' não informado.")]
        public long Id { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "O 'Amount' deve ser um número positivo.")]
        public double? Amount { get; set; }
        public DateTime? PurchaseDate { get; set; }

        [MaxLength(80, ErrorMessage = "Campo 'Destination' pode conter até 80 caracteres")]
        public string? Destination { get; set; }

        [MaxLength(100, ErrorMessage = "Campo 'Description' pode conter até 100 caracteres")]
        public string? Description { get; set; }

        [MaxLength(300, ErrorMessage = "Campo 'Observations' pode conter até 300 caracteres")]
        public String? Observations { get; set; }
        
        public bool? JustForRecord { get; set; }

        public long? CategoryId { get; set; }
    }
}
