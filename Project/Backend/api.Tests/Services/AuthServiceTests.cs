using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using api.DTO;
using api.Interfaces;
using api.Model;
using api.Repositories;
using api.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace api.Tests.Unit
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IConfiguration> _configMock;
        private readonly AuthService _service;

        public AuthServiceTests()
        {
            _userRepoMock = new Mock<IUserRepository>();
            _configMock = new Mock<IConfiguration>();

            // Setup fake JWT config section
            var jwtSection = new Mock<IConfigurationSection>();
            jwtSection.Setup(s => s["Key"]).Returns("supersecretkeysupersecretkey");
            jwtSection.Setup(s => s["Issuer"]).Returns("TestIssuer");
            jwtSection.Setup(s => s["Audience"]).Returns("TestAudience");
            jwtSection.Setup(s => s["ExpiresInMinutes"]).Returns("60");
            _configMock.Setup(c => c.GetSection("Jwt")).Returns(jwtSection.Object);

            _service = new AuthService(_userRepoMock.Object, _configMock.Object);
        }

        [Theory]
        [InlineData("test@example.com", true)]
        [InlineData("invalid@", false)]
        [InlineData("", false)]
        public void IsValidEmail_ReturnsExpectedResult(string email, bool expected)
        {
            var result = _service.IsValidEmail(email);
            Assert.Equal(expected, result);
        }

        

        [Fact]
        public async Task AuthenticateAsync_ReturnsNull_WhenInvalidPassword()
        {
            var user = new User
            {
                Username = "user1",
                Password = BCrypt.Net.BCrypt.HashPassword("correct"),
                Role = "user"
            };

            var login = new LoginRequestDTO
            {
                Username = "user1",
                Password = "wrong"
            };

            _userRepoMock.Setup(r => r.GetByUsername(login.Username)).ReturnsAsync(user);

            var result = await _service.AuthenticateAsync(login);

            Assert.Null(result);
        }

        [Fact]
        public async Task AuthenticateAsync_ReturnsNull_WhenUserNotFound()
        {
            var login = new LoginRequestDTO
            {
                Username = "missing",
                Password = "password"
            };

            _userRepoMock.Setup(r => r.GetByUsername(login.Username)).ReturnsAsync((User)null);

            var result = await _service.AuthenticateAsync(login);

            Assert.Null(result);
        }

        [Fact]
        public async Task RegisterAsync_CreatesUser_WhenValid()
        {
            var dto = new RegisterRequestDTO
            {
                Username = "newuser",
                Password = "securepass",
                Email = "user@gmail.com"
            };

            _userRepoMock.Setup(r => r.GetByUsername(dto.Username)).ReturnsAsync((User)null);
            _userRepoMock.Setup(r => r.CreateUser(It.IsAny<User>())).ReturnsAsync(new User { Id = 1, Username = dto.Username, Password="pass", Role="user" });

            var result = await _service.RegisterAsync(dto);

            Assert.NotNull(result);
            Assert.Equal(dto.Username, result.Username);
            _userRepoMock.Verify(r => r.CreateUser(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_Throws_WhenUsernameExists()
        {
            var dto = new RegisterRequestDTO
            {
                Username = "existing",
                Password = "pass"
            };

            _userRepoMock.Setup(r => r.GetByUsername(dto.Username)).ReturnsAsync(new User { Username = dto.Username, Password="pass", Role="user" });

            await Assert.ThrowsAsync<ArgumentException>(() => _service.RegisterAsync(dto));
        }

        [Theory]
        [InlineData(null, "pass", "Username cant be null or blank")]
        [InlineData("", "pass", "Username cant be null or blank")]
        [InlineData("user", "", "Password cant be null or blank")]
        public async Task RegisterAsync_Throws_OnInvalidFields(string username, string password, string expectedMessage)
        {
            var dto = new RegisterRequestDTO { Username = username, Password = password };

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.RegisterAsync(dto));
            Assert.Contains(expectedMessage, ex.Message);
        }

        [Fact]
        public async Task RegisterAsync_Throws_WhenEmailBlank()
        {
            var dto = new RegisterRequestDTO
            {
                Username = "user",
                Password = "pass",
                Email = ""
            };

            await Assert.ThrowsAsync<ArgumentException>(() => _service.RegisterAsync(dto));
        }
    }
}
