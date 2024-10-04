using System.ComponentModel.DataAnnotations;

namespace ControleCerto.DTOs.User
{
    public class UpdateUserRequest
    {
        [Required(ErrorMessage = "Id do usuário não informado.")]
        public int Id { get; set; }

        [MinLength(5, ErrorMessage = "Campo 'Name' precisa conter pelo menos 5 caracteres")]
        [MaxLength(100, ErrorMessage = "Campo 'Name' pode conter até 100 caracteres")]
        public string? Name { get; set; }

        [MinLength(7, ErrorMessage = "Campo 'Email' precisa conter pelo menos 7 caracteres")]
        [MaxLength(60, ErrorMessage = "Campo 'Email' pode conter até 60 caracteres")]
        public string? Email { get; set; }
    }
}
