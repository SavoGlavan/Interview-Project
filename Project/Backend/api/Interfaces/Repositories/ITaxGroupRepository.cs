using System.Collections.Generic;
using System.Threading.Tasks;
using api.DTO;
using api.Model;

namespace api.Repositories
{
    public interface ITaxGroupRepository
    {
        Task<List<TaxGroup>> getAll();
        Task<TaxGroup?> GetById(int id);
        Task<TaxGroup> CreateTaxGroup(TaxGroup taxGroup);
        Task<TaxGroup?> UpdateTaxGroup(int id, UpdateTaxGroupDTO dto);
        Task<bool> DeleteTaxGroup(int id);
    }
}
