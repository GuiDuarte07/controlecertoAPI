﻿﻿﻿using ControleCerto.DTOs.Account;
using ControleCerto.Enums;

namespace ControleCerto.DTOs.User
{
    public class InfoUserResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string? AvatarUrl { get; set; }
        public bool IsAdmin { get; set; }
        public UserTypeEnum UserType { get; set; }
    }
}
