using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.DTO
{
    public class UpdatePriceThresholdDTO
    {
        public int? Id { get; set; }  // null or 0 = new item
        public decimal? Price { get; set; }
        public int? Threshold { get; set; }
    }
}