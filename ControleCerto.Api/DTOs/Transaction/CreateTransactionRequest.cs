using ControleCerto.Enums;
using ControleCerto.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace ControleCerto.DTOs.Transaction
{
    public class CreateTransactionRequest
    {
        [Range(0, double.MaxValue, ErrorMessage = "O 'Amount' deve ser um número positivo.")]
        [Required(ErrorMessage = "Campo 'Amount' não informado.")]
        public double Amount { get; set; }

        [Required(ErrorMessage = "Campo 'Type' não informado.")]
        public TransactionTypeEnum Type { get; set; }

        [Required(ErrorMessage = "Campo 'PurchaseDate' não informado.")]
        public DateTime PurchaseDate { get; set; }

        [Required(ErrorMessage = "Campo 'Destination' não informado.")]
        [MaxLength(80, ErrorMessage = "Campo 'Destination' pode conter até 80 caracteres")]
        public string Destination { get; set; }

        [MaxLength(100, ErrorMessage = "Campo 'Description' pode conter até 100 caracteres")]
        public string Description { get; set; }

        [MaxLength(300, ErrorMessage = "Campo 'Description' pode conter até 300 caracteres")]
        public String? Observations { get; set; }

        [Required(ErrorMessage = "Campo 'AccountId' não informado.")]
        public long AccountId { get; set; }

        [Required(ErrorMessage = "Campo 'CategoryId' não informado.")]
        public long CategoryId { get; set; }

        [Required(ErrorMessage = "Campo 'JustForRecord' não informado.")]
        public bool JustForRecord { get; set; }
    }
}
