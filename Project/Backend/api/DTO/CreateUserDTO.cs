using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Model;

namespace api.DTO
{
    public class CreateUserDTO
    {
        public required string Username { get; set; }
        public required string Password { get; set; } 
        public required string Role { get; set; } 
        public double? Consumption { get; set; } 
        public int? TaxGroupId { get; set; }

        public string? Email { get; set; }
       
        public int? PlanId { get; set; }
        
    }
}