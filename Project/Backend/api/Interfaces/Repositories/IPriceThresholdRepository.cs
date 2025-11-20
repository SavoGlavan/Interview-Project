using System.Collections.Generic;
using System.Threading.Tasks;
using api.Model;

namespace api.Repositories
{
    public interface IPriceThresholdRepository
    {
        Task<List<PriceThreshold>> getAll();
        Task<PriceThreshold?> GetById(int id);
    }
}
