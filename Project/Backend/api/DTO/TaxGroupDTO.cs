using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.DTO
{
    public class TaxGroupDTO
    {
        public int Id { get; set; }
        public required string  Name { get; set; }
        public decimal Vat { get; set; }
        public decimal EcoTax { get; set; }
    }
}