using System.ComponentModel.DataAnnotations;

namespace ControleCerto.DTOs.CreditCard
{
    public class CreateCreditCardRequest
    {

        [Required(ErrorMessage = "Campo 'TotalLimit' não informado.")]
        public double TotalLimit { get; set; }

        [MaxLength(100, ErrorMessage = "Campo 'Description' pode conter até 100 caracteres")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Campo 'AccountId' não informado.")]
        public int AccountId { get; set; }
        
        [Required(ErrorMessage = "Campo 'CloseDay' não informado.")]
        [Range(1, 20, ErrorMessage = "Data de fechamento precisa está entre o dia 2 e o dia 28")]
        public int CloseDay { get; set; }

        [Required(ErrorMessage = "Campo 'DueDay' não informado.")]
        [Range(2, 28, ErrorMessage = "Data de fechamento precisa está entre o dia 2 e o dia 28")]
        public int DueDay { get; set; }

        [Required(ErrorMessage = "Campo 'SkipWeekend' não informado.")]
        public bool SkipWeekend { get; set; }

    }
}
