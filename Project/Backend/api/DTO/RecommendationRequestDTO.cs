using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.DTO
{
    public class RecommendationRequestDTO
{
    public double Consumption { get; set; }
    public int TaxGroupId { get; set; }
}

}