using System.Collections.Generic;
using System.Threading.Tasks;
using api.DTO;

namespace api.Interfaces
{
    public interface ITaxGroupService
    {
        Task<List<TaxGroupDTO>> getAllTaxGroups();
        Task<TaxGroupDTO?> getTaxGroupById(int id);
        Task<TaxGroupDTO> CreateTaxGroup(CreateTaxGroupDTO dto);
        Task<TaxGroupDTO?> UpdateTaxGroup(int id, UpdateTaxGroupDTO dto);
        Task<bool> DeleteTaxGroup(int id);
    }
}
