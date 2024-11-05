using System.ComponentModel.DataAnnotations;

namespace ControleCerto.DTOs.TransferenceDTO
{
    public class CreateTransferenceRequest
    {
        [MaxLength(100, ErrorMessage = "Campo 'Description' pode conter até 100 caracteres")]
        public string? Description { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "O 'Amount' deve ser um número positivo.")]
        [Required(ErrorMessage = "Campo 'Amount' não informado.")]
        public double Amount { get; set; }

        [Required(ErrorMessage = "Campo 'PurchaseDate' não informado.")]
        public string PurchaseDate { get; set; }

        [Required(ErrorMessage = "Campo 'AccountDestinyId' não informado.")]
        public long AccountDestinyId { get; set; }

        [Required(ErrorMessage = "Campo 'AccountOriginId' não informado.")]
        public long AccountOriginId { get; set; }
    }
}
