using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.DTO
{
    public class RegisterRequestDTO
   {
        public required string Username { get; set; }
        public required string Password { get; set; }
        public  string? Email { get; set; }
        public string Role { get; set; } = "user"; // default to normal user
    }
}