using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.DTO
{
    public class CreatePlanDTO
    {
        public required string Name { get; set; }
        public List<CreatePriceThresholdDTO> Prices { get; set; } = new();
        public decimal? Discount { get; set; }
    }
}