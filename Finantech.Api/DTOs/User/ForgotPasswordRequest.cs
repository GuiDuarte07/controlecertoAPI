using System.ComponentModel.DataAnnotations;

namespace Finantech.DTOs.User
{
    public class ForgotPasswordRequest
    {
        [Required(ErrorMessage = "Campo 'Password' não informado.")]
        [MinLength(4, ErrorMessage = "Campo 'Password' precisa conter pelo menos 4 caracteres")]
        [MaxLength(30, ErrorMessage = "Campo 'Password' pode conter até 30 caracteres")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Campo 'ConfirmPassword' não informado.")]
        [MinLength(4, ErrorMessage = "Campo 'ConfirmPassword' precisa conter pelo menos 4 caracteres")]
        [MaxLength(30, ErrorMessage = "Campo 'ConfirmPassword' pode conter até 30 caracteres")]
        public string ConfirmPassword { get; set; }
    }
}
