using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using api.Controllers;
using api.Data;
using api.DTO;
using api.Model;
using api.Repositories;
using api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace api.Tests.Integration
{
    public class AuthIntegrationTests
    {
        private readonly ApplicationDBContext _context;
        private readonly UserRepository _userRepository;
        private readonly AuthService _authService;
        private readonly AuthController _controller;

        public AuthIntegrationTests()
        {
            // Use in-memory DB for integration
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDBContext(options);
            _userRepository = new UserRepository(_context);

            // Mock configuration with realistic JWT settings
            var inMemorySettings = new Dictionary<string, string>
            {
                { "Jwt:Key", "supersecretkeysupersecretkey123123123" },
                { "Jwt:Issuer", "TestIssuer" },
                { "Jwt:Audience", "TestAudience" },
                { "Jwt:ExpiresInMinutes", "60" }
            };
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _authService = new AuthService(_userRepository, configuration);
            _controller = new AuthController(_authService);
        }

        [Fact]
        public async Task Register_Then_Login_Succeeds()
        {
            // Arrange: register a user
            var registerDto = new RegisterRequestDTO
            {
                Username = "testuser",
                Password = "mypassword",
                Email = "user@example.com"
            };

            // Act: register
            var registerResult = await _controller.Register(registerDto);
            var okResult = Assert.IsType<OkObjectResult>(registerResult);

            // Assert registration succeeded
            Assert.Equal(200, okResult.StatusCode);

            // Act: login
            var loginDto = new LoginRequestDTO
            {
                Username = "testuser",
                Password = "mypassword"
            };
            var loginResult = await _controller.Login(loginDto);

            // Assert login returned token
            var loginOk = Assert.IsType<OkObjectResult>(loginResult);
            var token = loginOk.Value?.GetType().GetProperty("token")?.GetValue(loginOk.Value)?.ToString();

            Assert.NotNull(token);
            Assert.Contains(".", token); // basic check for JWT structure
        }

        [Fact]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange: register a user
            var registerDto = new RegisterRequestDTO
            {
                Username = "user1",
                Password = "correctpass"
            };
            await _controller.Register(registerDto);

            // Act: try login with wrong password
            var loginDto = new LoginRequestDTO
            {
                Username = "user1",
                Password = "wrongpass"
            };
            var result = await _controller.Login(loginDto);

            // Assert unauthorized
            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            var message = unauthorized.Value?.GetType().GetProperty("message")?.GetValue(unauthorized.Value)?.ToString();

            Assert.Equal(401, unauthorized.StatusCode);
            Assert.Contains("Invalid credentials", message);
        }

        [Fact]
        public async Task Register_ExistingUsername_ReturnsConflict()
        {
            // Arrange: create a user manually
            var existingUser = new User
            {
                Username = "duplicate",
                Password = BCrypt.Net.BCrypt.HashPassword("pass"),
                Role = "user",
                Email = "dup@example.com"
            };
            _context.Users.Add(existingUser);
            await _context.SaveChangesAsync();

            // Act: attempt to register with same username
            var registerDto = new RegisterRequestDTO
            {
                Username = "duplicate",
                Password = "newpass",
                Email = "dup2@example.com"
            };

            var result = await _controller.Register(registerDto);

            // Assert conflict
            var conflict = Assert.IsType<ConflictObjectResult>(result);
            var message = conflict.Value?.GetType().GetProperty("message")?.GetValue(conflict.Value)?.ToString();

            Assert.Equal(409, conflict.StatusCode);
            Assert.Contains("Username already taken", message);
        }

        [Fact]
        public async Task Register_InvalidEmail_ReturnsConflict()
        {
            var dto = new RegisterRequestDTO
            {
                Username = "invalidemail",
                Password = "pass",
                Email = "" // invalid case
            };

            var result = await _controller.Register(dto);
            var conflict = Assert.IsType<ConflictObjectResult>(result);

            var message = conflict.Value?.GetType().GetProperty("message")?.GetValue(conflict.Value)?.ToString();
            Assert.Equal(409, conflict.StatusCode);
            Assert.Contains("Email cant be blank", message);
        }
    }
}
