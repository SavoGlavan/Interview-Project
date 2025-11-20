using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Model
{
    public class Plan
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public decimal? Discount { get; set; }
        public List<PriceThreshold> Prices { get; set; } = new List<PriceThreshold>();
    }
}