using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.DTO
{
    public class UpdatePlanDTO
{
    public string? Name { get; set; }
    public decimal? Discount { get; set; }

    public List<UpdatePriceThresholdDTO> Prices { get; set; } = new();
}

}