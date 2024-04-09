using System.ComponentModel.DataAnnotations;

namespace Finantech.DTOs.Auth
{
    public class LoginRquest
    {
        [Required(ErrorMessage = "Campo 'Email' não informado.")]
        [MaxLength(60, ErrorMessage = "Campo 'Name' pode conter até 60 caracteres")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Campo 'Password' não informado.")]
        [MaxLength(30, ErrorMessage = "Campo 'Password' pode conter até 30 caracteres")]
        public string Password { get; set; }
    }
}
