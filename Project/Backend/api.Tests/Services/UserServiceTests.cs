using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using api.DTO;
using api.Interfaces;
using api.Model;
using api.Repositories;
using api.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace api.Tests.Unit
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<ITaxGroupRepository> _taxGroupRepoMock;
        private readonly Mock<IPlanRepository> _planRepoMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly UserService _service;

        public UserServiceTests()
        {
            _userRepoMock = new Mock<IUserRepository>();
            _taxGroupRepoMock = new Mock<ITaxGroupRepository>();
            _planRepoMock = new Mock<IPlanRepository>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            // Default context with a logged-in user ID = 1
            var claims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1")
            }));
            _httpContextAccessorMock.Setup(x => x.HttpContext.User).Returns(claims);

            _service = new UserService(
                _userRepoMock.Object,
                _httpContextAccessorMock.Object,
                _taxGroupRepoMock.Object,
                _planRepoMock.Object
            );
        }

        [Fact]
        public async Task GetAllUsers_ReturnsMappedUsers()
        {
            _userRepoMock.Setup(r => r.getAll()).ReturnsAsync(new List<User>
            {
                new User { Id = 1, Username = "user1", Email = "a@test.com", Role="user", Password="pass" }
            });

            var users = await _service.getAllUsers();

            Assert.Single(users);
            Assert.Equal("user1", users[0].Username);
            Assert.Equal("a@test.com", users[0].Email);
        }

        [Fact]
        public async Task GetUserById_ReturnsMappedUser()
        {
            _userRepoMock.Setup(r => r.GetById(1)).ReturnsAsync(new User
            {
                Id = 1, Username = "user1", Email = "a@test.com", Role="user", Password="pass"
            });

            var user = await _service.getUserById(1);

            Assert.NotNull(user);
            Assert.Equal(1, user.Id);
            Assert.Equal("user1", user.Username);
        }

        

        [Fact]
        public async Task CreateUser_ReturnsCreatedUserDTO()
        {
            var dto = new CreateUserDTO
            {
                Username = "newuser",
                Password = "pass",
                Role = "user",
                Consumption = 100,
                Email = "test@test.com",
                TaxGroupId = null
            };

            _userRepoMock.Setup(r => r.CreateUser(It.IsAny<User>())).ReturnsAsync(new User
            {
                Id = 1,
                Username = "newuser",
                Role = "user",
                Consumption = 100,
                Email = "test@test.com",
                Password="pass"
            });

            var result = await _service.CreateUser(dto);

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("newuser", result.Username);
        }

        [Fact]
        public async Task UpdateUser_ThrowsIfUserNotLoggedInOrDifferentId()
        {
            // User with ID 2 trying to update ID 3
            _userRepoMock.Setup(r => r.GetById(3)).ReturnsAsync(new User { Id = 3, Username="user1", Password="pass1", Role="user" });
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _service.UpdateUser(3, new UpdateUserDTO())
            );
        }

      
        [Fact]
        public async Task DeleteUser_Throws_WhenNotFound()
        {
            _userRepoMock.Setup(r => r.DeleteUser(2)).ReturnsAsync(false);

            await Assert.ThrowsAsync<Exception>(() => _service.DeleteUser(2));
        }

        [Fact]
        public void IsValidEmail_ReturnsTrue_ForValidEmail()
        {
            var valid = _service.IsValidEmail("a@test.com");
            Assert.True(valid);
        }

        [Fact]
        public void IsValidEmail_ReturnsFalse_ForInvalidEmail()
        {
            var valid = _service.IsValidEmail("notanemail");
            Assert.False(valid);
        }
    }
}
