using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.DTO
{
    public class UpdateUserDTO
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
        
        public double? Consumption { get; set; }
        public int? TaxGroupId { get; set; }
        public int? PlanId { get; set; }
        public string? Email { get; set; }
    }
}