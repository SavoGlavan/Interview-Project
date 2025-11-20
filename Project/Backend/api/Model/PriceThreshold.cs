using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Model
{
    public class PriceThreshold
    {
        public int Id { get; set; }
        public decimal Price { get; set; }
        public int? Threshold { get; set; }
        public int PlanId { get; set; }
        public Plan Plan { get; set; } = null!;

    }
}