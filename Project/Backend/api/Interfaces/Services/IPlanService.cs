using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using api.DTO;
using api.Model;

namespace api.Interfaces
{
    public interface IPlanService
    {
        Task<List<PlanDTO>> getAllPlans();
        Task<PlanDTO?> getPlanById(int id);
        Task<PlanDTO> CreatePlan(CreatePlanDTO dto);
        Task<PlanDTO?> UpdatePlan(int id, UpdatePlanDTO dto);
        Task<bool> DeletePlan(int id);
        Task<RecomendationResponseDTO> RecommendPlan(double userConsumption, int taxGroupId);
        decimal CalculateTotalCost(Plan plan, double consumption, TaxGroup taxGroup);
    }
}
