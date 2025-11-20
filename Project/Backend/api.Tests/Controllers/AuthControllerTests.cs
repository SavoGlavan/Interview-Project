using System;
using System.Threading.Tasks;
using api.Controllers;
using api.DTO;
using api.Interfaces;
using api.Model;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using System.Reflection;

namespace api.Tests.Unit
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _authServiceMock = new Mock<IAuthService>();
            _controller = new AuthController(_authServiceMock.Object);
        }

        [Fact]
        public async Task Login_ReturnsOk_WithValidCredentials()
        {
            var dto = new LoginRequestDTO { Username = "user", Password = "pass" };
            _authServiceMock.Setup(s => s.AuthenticateAsync(dto)).ReturnsAsync("fake-jwt-token");

            var result = await _controller.Login(dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var token = GetPropertyValue(okResult.Value, "token");

            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal("fake-jwt-token", token);
        }

        [Fact]
        public async Task Login_ReturnsUnauthorized_WithInvalidCredentials()
        {
            var dto = new LoginRequestDTO { Username = "user", Password = "wrong" };
            _authServiceMock.Setup(s => s.AuthenticateAsync(dto)).ReturnsAsync((string)null);

            var result = await _controller.Login(dto);

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            var message = GetPropertyValue(unauthorized.Value, "message");

            Assert.Equal(401, unauthorized.StatusCode);
            Assert.Contains("Invalid credentials", message);
        }

        [Fact]
        public async Task Register_ReturnsOk_WhenSuccessful()
        {
            var dto = new RegisterRequestDTO { Username = "newuser", Password = "password" };
            var user = new User { Username = "newuser", Password = "password", Role = "user" };

            _authServiceMock.Setup(s => s.RegisterAsync(dto)).ReturnsAsync(user);

            var result = await _controller.Register(dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var message = GetPropertyValue(okResult.Value, "message");
            var username = GetPropertyValue(okResult.Value, "Username");

            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal("User registered successfully", message);
            Assert.Equal("newuser", username);
        }

        [Fact]
        public async Task Register_ReturnsConflict_OnArgumentException()
        {
            var dto = new RegisterRequestDTO { Username = "exists", Password = "pass" };
            _authServiceMock.Setup(s => s.RegisterAsync(dto))
                .ThrowsAsync(new ArgumentException("Username already taken"));

            var result = await _controller.Register(dto);

            var conflict = Assert.IsType<ConflictObjectResult>(result);
            var message = GetPropertyValue(conflict.Value, "message");

            Assert.Equal(409, conflict.StatusCode);
            Assert.Contains("Username already taken", message);
        }

        private string GetPropertyValue(object obj, string propertyName)
        {
            return obj?.GetType()
                .GetProperty(propertyName)?
                .GetValue(obj)?
                .ToString();
        }
    }
}