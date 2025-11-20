using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.Interfaces;
using api.Model;
using Microsoft.EntityFrameworkCore;

namespace api.Repositories
{
    public class PriceThresholdRepository:IPriceThresholdRepository
      {
        private readonly ApplicationDBContext _context;
        public PriceThresholdRepository(ApplicationDBContext context)
        {
            _context = context;
        }
        public async Task<List<PriceThreshold>> getAll()
        {
            return await _context.PriceThresholds
            .Include(p => p.Plan)
            .ToListAsync();
        }
        
        public async Task<PriceThreshold?> GetById(int id)
        {
              return await _context.PriceThresholds
                .FirstOrDefaultAsync(p => p.Id == id); 
        }
    }
}