using System;
using System.Collections.Generic;
using System.Text;
using TaskAPI.Entities.Enums;

namespace TaskAPI.Entities.Dtos
{
    public class LoginResponseDto
    {
        public string? Username { get; set; }
        public LoginResultEnum Result { get; set; }
        public string? Token { get; set; }
    }
}
