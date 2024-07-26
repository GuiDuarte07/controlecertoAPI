using System.ComponentModel.DataAnnotations;

namespace Finantech.DTOs.User
{
    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "Campo 'OldPassword' não informado.")]
        [MinLength(4, ErrorMessage = "Campo 'OldPassword' precisa conter pelo menos 4 caracteres")]
        [MaxLength(30, ErrorMessage = "Campo 'OldPassword' pode conter até 30 caracteres")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "Campo 'NewPassword' não informado.")]
        [MinLength(4, ErrorMessage = "Campo 'NewPassword' precisa conter pelo menos 4 caracteres")]
        [MaxLength(30, ErrorMessage = "Campo 'NewPassword' pode conter até 30 caracteres")]
        public string NewPassword { get; set; }

        public ChangePasswordRequest(string oldPassword, string newPassword)
        {
            OldPassword = oldPassword;
            NewPassword = newPassword;
        }

        public ChangePasswordRequest()
        {
        }
    }
}
