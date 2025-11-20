using api.Repositories;

using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using api.Model;
using api.DTO;
using System.Net.Mail;
using api.Interfaces;

namespace api.Services
{   


    public class AuthService:IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public AuthService(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public bool IsValidEmail(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
        public async Task<string?> AuthenticateAsync(LoginRequestDTO login)
        {
            var user = await _userRepository.GetByUsername(login.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(login.Password, user.Password))
                return null;

            return GenerateJwtToken(user);
        }


        public async Task<User?> RegisterAsync(RegisterRequestDTO registerDto)
        {   
            //---Validation---
            if(string.IsNullOrWhiteSpace(registerDto.Username))
                throw new ArgumentException("Username cant be null or blank");
            if(string.IsNullOrWhiteSpace(registerDto.Password))
                throw new ArgumentException("Password cant be null or blank");
            if(registerDto.Email!=null && registerDto.Email=="")
                throw new ArgumentException("Email cant be blank");
            if(registerDto.Email!=null && !IsValidEmail(registerDto.Email))
                throw new ArgumentException("Email not in correct format");
            // check if username already exists
            var existingUser = await _userRepository.GetByUsername(registerDto.Username);
            if (existingUser != null)
                throw new ArgumentException("Username already taken");

            // hash the password
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

            var user = new User
            {
                Username = registerDto.Username,
                Password = hashedPassword,
                Role = "user",
                Email = registerDto.Email
            };
            
            await _userRepository.CreateUser(user);

            return user;
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username)
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpiresInMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
