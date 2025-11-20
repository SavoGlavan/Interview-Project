using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.DTO;
using api.Interfaces;
using api.Model;
using Microsoft.EntityFrameworkCore;

namespace api.Repositories
{
    public class PlanRepository:IPlanRepository
    {
        private readonly ApplicationDBContext _context;
        public PlanRepository(ApplicationDBContext context)
        {
            _context = context;
        }
        public async Task<List<Plan>> getAll()
        {
            return await _context.Plans.Include(p => p.Prices)
           .ToListAsync();
        }
        
        public async Task<Plan?> GetById(int id)
        {
            return await _context.Plans
                .Include(p => p.Prices)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Plan> CreatePlan(Plan plan)
        {
            _context.Plans.Add(plan);
            await _context.SaveChangesAsync();
            return plan;
        }
        
        public async Task<Plan?> UpdatePlan(int id, UpdatePlanDTO dto)
        {
            var plan = await _context.Plans
                .Include(p => p.Prices)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (plan == null)
                return null;

            // Update plan fields
            plan.Name = dto.Name ?? plan.Name;
            plan.Discount = dto.Discount ?? plan.Discount;

            // Handle price thresholds
            var updatedIds = new List<int>();

            foreach (var priceDto in dto.Prices)
            {
                if (priceDto.Id.HasValue && priceDto.Id > 0)
                {
                    // Update existing price
                    var existing = plan.Prices.FirstOrDefault(p => p.Id == priceDto.Id.Value);
                    if (existing != null)
                    {
                        existing.Price = priceDto.Price ?? existing.Price;
                        existing.Threshold = priceDto.Threshold ?? existing.Threshold;
                        updatedIds.Add(existing.Id);
                    }
                }
                else
                {
                    // New price threshold
                    var newPrice = new PriceThreshold
                    {
                        Price = (decimal) priceDto.Price,
                        Threshold = priceDto.Threshold ?? null,
                        PlanId = plan.Id
                    };
                    plan.Prices.Add(newPrice);
                }
            }

            // Remove thresholds that were not included in the DTO
            var toRemove = plan.Prices
                .Where(p => !updatedIds.Contains(p.Id) && !dto.Prices.Any(dp => dp.Id == p.Id))
                .ToList();
            toRemove = toRemove.Where(p => p.Id > 0).ToList();
            _context.PriceThresholds.RemoveRange(toRemove);

            await _context.SaveChangesAsync();
            return plan;
        }
        public async Task<bool> DeletePlan(int id)
        {
            var plan = await _context.Plans
                .Include(p => p.Prices)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (plan == null)
                return false;

            // Remove thresholds manually
            _context.PriceThresholds.RemoveRange(plan.Prices);

            // Remove the plan itself
            _context.Plans.Remove(plan);

            await _context.SaveChangesAsync();
            return true;
        }

    }
}