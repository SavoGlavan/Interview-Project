using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using api.Data;
using api.DTO;
using api.Model;
using Microsoft.EntityFrameworkCore;
namespace api.Repositories
{
    public class UserRepository:IUserRepository
    {
        private readonly ApplicationDBContext _context;

        public UserRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<List<User>> getAll()
        {
            return await _context.Users.
            Include(u => u.TaxGroup)
            .Include(u => u.Plan)
           .ToListAsync();
        }

        public async Task<User?> GetById(int id)
        {
            return await _context.Users
                .Include(u => u.TaxGroup)
                .Include(u => u.Plan)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetByUsername(string username)
        {
            return await _context.Users
                .Include(u => u.TaxGroup)
                .Include(u => u.Plan)
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<bool>  planHasUsers(int planId)
        {
            return await _context.Users.AnyAsync(u => u.PlanId == planId) ;
        }

        public async Task<bool>  taxGroupHasUsers(int taxGroupId)
        {
            return await _context.Users.AnyAsync(u => u.TaxGroupId == taxGroupId) ;
        }
        public async Task<User> CreateUser(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Reload with navigation properties
            return await _context.Users
                .Include(u => u.TaxGroup)
                .Include(u => u.Plan)
                .FirstOrDefaultAsync(u => u.Id == user.Id);
        }

        public async Task<IEnumerable<object>> GetUserCountByTaxGroup()
        {
            var result = await _context.Users
                .Where(u => u.TaxGroupId != null)
                .GroupBy(u => u.TaxGroup.Name)
                .Select(g => new
                {
                    TaxGroup = g.Key,
                    UserCount = g.Count()
                })
                .ToListAsync();

            return result;
        }

        public async Task<IEnumerable<object>> GetUserCountByPlan()
        {
            var result = await _context.Users
                .Where(u => u.PlanId != null)
                .GroupBy(u => u.Plan.Name)
                .Select(g => new
                {
                    Plan = g.Key,
                    UserCount = g.Count()
                })
                .ToListAsync();

            return result;
        }



        public async Task<User?> UpdateUser(int id, UpdateUserDTO dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return null;

            // Apply only non-null updates
            if (!string.IsNullOrEmpty(dto.Username))
                user.Username = dto.Username;

            if (!string.IsNullOrEmpty(dto.Password))
                user.Password = dto.Password;

            if (dto.Consumption.HasValue)
                user.Consumption = dto.Consumption.Value;

            if (dto.TaxGroupId.HasValue)
                user.TaxGroupId = dto.TaxGroupId.Value;

            if (dto.PlanId.HasValue)
                user.PlanId = dto.PlanId.Value;
            
            if (!string.IsNullOrEmpty(dto.Email))
                user.Email = dto.Email;

            await _context.SaveChangesAsync();

            // Include related entities for DTO mapping later
            await _context.Entry(user).Reference(u => u.TaxGroup).LoadAsync();
            await _context.Entry(user).Reference(u => u.Plan).LoadAsync();

            return user;
        }
        
        public async Task<bool> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

    }
}