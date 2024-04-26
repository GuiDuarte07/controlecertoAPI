using Finantech.Enums;
using System.ComponentModel.DataAnnotations;

namespace Finantech.DTOs.Account
{
    public class UpdateIncomeRequest
    {
        [Required(ErrorMessage = "Campo 'Id' não informado.")]
        public int Id { get; set; }

        public double? Amount { get; set; }

        public IncomeTypeEnum? IncomeType { get; set; }

        public DateTime PurchaseDate { get; set; }

        [MaxLength(80, ErrorMessage = "Campo 'Description' pode conter até 80 caracteres")]
        public string? Origin { get; set; }

        public string? Description { get; set; }
        public int CategoryId { get; set; }
    }
}
