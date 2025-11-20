using api.DTO;
using api.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace api.Interfaces
{
    public interface IPlanRepository
    {
        Task<List<Plan>> getAll();
        Task<Plan?> GetById(int id);
        Task<Plan> CreatePlan(Plan plan);
        Task<Plan?> UpdatePlan(int id, UpdatePlanDTO dto);
        Task<bool> DeletePlan(int id);
    }
}
