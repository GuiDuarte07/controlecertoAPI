﻿using System.ComponentModel.DataAnnotations;

namespace Finantech.DTOs.Auth
{
    public class AuthRequest
    {
        [Required(ErrorMessage = "Campo 'Email' não informado.")]
        [MinLength(4, ErrorMessage = "Campo 'Email' precisa conter pelo menos 4 caracteres")]
        [MaxLength(60, ErrorMessage = "Campo 'Email' pode conter até 60 caracteres")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Campo 'Password' não informado.")]
        [MinLength(4, ErrorMessage = "Campo 'Password' precisa conter pelo menos 4 caracteres")]
        [MaxLength(30, ErrorMessage = "Campo 'Password' pode conter até 30 caracteres")]
        public string Password { get; set; }
    }
}
