using System.Collections.Generic;
using System.Threading.Tasks;
using api.DTO;

namespace api.Interfaces
{
    public interface IUserService
    {
        Task<List<UserDTO>> getAllUsers();
        Task<UserDTO?> getUserById(int id);
        Task<UserDTO?> getUserByUsername(string username);
        Task<UserDTO> CreateUser(CreateUserDTO dto);
        Task<UserDTO?> UpdateUser(int id, UpdateUserDTO dto);
        Task<bool> DeleteUser(int id);
        Task<IEnumerable<object>> GetUserCountByTaxGroup();
        Task<IEnumerable<object>> GetUserCountByPlan();
        bool IsValidEmail(string email);
    }
}
