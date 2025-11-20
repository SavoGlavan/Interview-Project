using api.DTO;
using api.Model;
using System.Threading.Tasks;

namespace api.Interfaces
{
    public interface IAuthService
    {
        bool IsValidEmail(string email);
        Task<string?> AuthenticateAsync(LoginRequestDTO login);
        Task<User?> RegisterAsync(RegisterRequestDTO registerDto);
    }
}
