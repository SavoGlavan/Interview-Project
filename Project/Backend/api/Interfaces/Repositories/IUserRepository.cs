using System.Collections.Generic;
using System.Threading.Tasks;
using api.DTO;
using api.Model;

namespace api.Repositories
{
    public interface IUserRepository
    {
        Task<List<User>> getAll();
        Task<User?> GetById(int id);
        Task<User?> GetByUsername(string username);
        Task<bool> planHasUsers(int planId);
        Task<bool> taxGroupHasUsers(int taxGroupId);
        Task<User> CreateUser(User user);
        Task<IEnumerable<object>> GetUserCountByTaxGroup();
        Task<IEnumerable<object>> GetUserCountByPlan();
        Task<User?> UpdateUser(int id, UpdateUserDTO dto);
        Task<bool> DeleteUser(int id);
    }
}
