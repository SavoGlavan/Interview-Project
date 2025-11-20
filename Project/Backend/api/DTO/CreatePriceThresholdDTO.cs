using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.DTO
{
    public class CreatePriceThresholdDTO
    {
        public decimal Price { get; set; }
        public int? Threshold { get; set; }
    }
}