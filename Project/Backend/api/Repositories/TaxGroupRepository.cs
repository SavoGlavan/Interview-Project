using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.DTO;
using api.Model;
using Microsoft.EntityFrameworkCore;
namespace api.Repositories
{
    public class TaxGroupRepository:ITaxGroupRepository
      {
        private readonly ApplicationDBContext _context;

        public TaxGroupRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<List<TaxGroup>> getAll()
        {
            return await _context.TaxGroups.ToListAsync();
        }

        public async Task<TaxGroup?> GetById(int id)
        {
            return await _context.TaxGroups
              .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<TaxGroup> CreateTaxGroup(TaxGroup taxGroup)
        {
            _context.TaxGroups.Add(taxGroup);
            await _context.SaveChangesAsync();
            return taxGroup;
        }
        public async Task<TaxGroup?> UpdateTaxGroup(int id, UpdateTaxGroupDTO dto)
        {
            var taxGroup = await _context.TaxGroups.FindAsync(id);
            if (taxGroup == null)
                return null;

            // Apply only non-null updates
            if (!string.IsNullOrEmpty(dto.Name))
                taxGroup.Name = dto.Name;

            if (dto.Vat.HasValue)
                taxGroup.Vat = dto.Vat.Value;

            if (dto.Eco_tax.HasValue)
                taxGroup.Eco_tax = dto.Eco_tax.Value;

            await _context.SaveChangesAsync();



            return taxGroup;
        }
        
        public async Task<bool> DeleteTaxGroup(int id)
        {
            var taxGroup = await _context.TaxGroups.FindAsync(id);
            if(taxGroup==null)
                return false;
            _context.TaxGroups.Remove(taxGroup);
            await _context.SaveChangesAsync();
            return true;
        }


    
    }
}