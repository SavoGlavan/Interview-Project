using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.DTO
{
    public class UserDetailDTO
    {
        public int? PlanId { get; set; }
        public  string? PlanName { get; set; }

        public int? TaxGroupId { get; set; }
        public  string? TaxGroupName { get; set; }
    }
}