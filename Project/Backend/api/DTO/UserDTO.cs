using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.DTO
{
    public class UserDTO
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string Role { get; set; }
        public double? Consumption { get; set; }
        public string? Email { get; set; }
        public TaxGroupDTO? TaxGroup { get; set; }
        public UserDetailDTO? Details { get; set; }
    }
}