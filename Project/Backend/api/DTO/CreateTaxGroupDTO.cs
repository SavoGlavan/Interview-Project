using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.DTO
{
    public class CreateTaxGroupDTO
    {
        public required string Name { get; set; }
        public decimal Vat { get; set; } 
        public decimal Eco_tax { get; set; } 
    }
}