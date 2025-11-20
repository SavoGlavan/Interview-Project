using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Model
{
    public class TaxGroup
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public decimal Vat { get; set; } 
        public decimal Eco_tax { get; set; } 
        
    }
}