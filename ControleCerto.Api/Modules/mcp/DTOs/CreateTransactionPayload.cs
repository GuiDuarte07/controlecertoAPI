using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ControleCerto.Enums;
using ControleCerto.Modules.Mcp.JsonConverters;

namespace ControleCerto.Modules.Mcp.DTOs
{
    public class CreateTransactionPayload
    {
        [Range(1, long.MaxValue, ErrorMessage = "Campo 'accountId' deve ser maior que zero.")]
        public long AccountId { get; set; }

        [Required(ErrorMessage = "Campo 'amount' não informado.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O campo 'amount' deve ser positivo.")]
        public double Amount { get; set; }

        [Required(ErrorMessage = "Campo 'purchaseDate' não informado.")]
        public DateTime PurchaseDate { get; set; }

        [Required(ErrorMessage = "Campo 'destination' não informado.")]
        [MaxLength(80, ErrorMessage = "Campo 'destination' pode conter até 80 caracteres.")]
        public string Destination { get; set; } = string.Empty;

        [MaxLength(100, ErrorMessage = "Campo 'description' pode conter até 100 caracteres.")]
        public string? Description { get; set; }

        [MaxLength(300, ErrorMessage = "Campo 'observations' pode conter até 300 caracteres.")]
        public string? Observations { get; set; }

        [Range(1, long.MaxValue, ErrorMessage = "Campo 'categoryId' deve ser maior que zero.")]
        public long CategoryId { get; set; }

        [Required(ErrorMessage = "Campo 'type' não informado.")]
        [JsonConverter(typeof(TransactionTypeEnumConverter))]
        public TransactionTypeEnum? Type { get; set; }

        [Required(ErrorMessage = "Campo 'justForRecord' não informado.")]
        public bool? JustForRecord { get; set; }
    }
}
