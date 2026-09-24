using System;
using System.Collections.Generic;
using System.Text;

namespace TaskAPI.Entities.Dtos
{
    public class LoginDto
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
    }
}
