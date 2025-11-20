using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.DTO
{
    public class PlanDTO
    {
        public int Id { get; set; }
        public required string Name { get; set; }

        public decimal? Discount { get; set; }
        public required List<PriceThresholdDTO> Prices { get; set; }
    }
}