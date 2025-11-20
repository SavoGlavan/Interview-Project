using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.DTO
{
    public class RecomendationResponseDTO
    {
        public PlanDTO? planDTO { get; set; }
        public decimal totalPrice { get; set; }
    }
}