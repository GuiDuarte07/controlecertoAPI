using System.ComponentModel.DataAnnotations;

namespace Finantech.DTOs.User
{
    public class CreateUserRequest
    {
        [Required(ErrorMessage = "Campo 'Name' não informado.")]
        [MaxLength(100, ErrorMessage = "Campo 'Name' pode conter até 100 caracteres")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Campo 'Email' não informado.")]
        [MaxLength(60, ErrorMessage = "Campo 'Name' pode conter até 60 caracteres")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Campo 'Password' não informado.")]
        [MaxLength(30, ErrorMessage = "Campo 'Password' pode conter até 30 caracteres")]
        public string Password { get; set; }
    }
}
