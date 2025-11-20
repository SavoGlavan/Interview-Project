using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using api.Controllers;
using api.DTO;
using api.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace api.Tests.Unit
{
    public class UserControllerTests
    {
        private readonly Mock<IUserService> _serviceMock;
        private readonly UserController _controller;

        public UserControllerTests()
        {
            _serviceMock = new Mock<IUserService>();
            _controller = new UserController(_serviceMock.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOk_WithListOfUsers()
        {
            var users = new List<UserDTO>
            {
                new UserDTO { Id = 1, Username = "user1", Role="user" },
                new UserDTO { Id = 2, Username = "user2", Role="user" }
            };

            _serviceMock.Setup(s => s.getAllUsers()).ReturnsAsync(users);

            var result = await _controller.GetAll();
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedUsers = Assert.IsAssignableFrom<List<UserDTO>>(okResult.Value);

            Assert.Equal(2, returnedUsers.Count);
        }

        [Fact]
        public async Task GetById_ReturnsOk_WhenUserExists()
        {
            var user = new UserDTO { Id = 1, Username = "user1", Role="user" };
            _serviceMock.Setup(s => s.getUserById(1)).ReturnsAsync(user);

            var result = await _controller.GetById(1);
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedUser = Assert.IsType<UserDTO>(okResult.Value);

            Assert.Equal(1, returnedUser.Id);
            Assert.Equal("user1", returnedUser.Username);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenUserDoesNotExist()
        {
            _serviceMock.Setup(s => s.getUserById(1)).ReturnsAsync((UserDTO)null);

            var result = await _controller.GetById(1);
            Assert.IsType<NotFoundResult>(result.Result);
        }

       

        [Fact]
        public async Task UpdateUser_ReturnsOk_WhenUpdateSucceeds()
        {
            var dto = new UpdateUserDTO { Username = "updated" };
            var updatedUser = new UserDTO { Id = 1, Username = "updated", Role="user" };

            _serviceMock.Setup(s => s.UpdateUser(1, dto)).ReturnsAsync(updatedUser);

            var result = await _controller.UpdateUser(1, dto);
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedUser = Assert.IsType<UserDTO>(okResult.Value);

            Assert.Equal("updated", returnedUser.Username);
        }

        [Fact]
        public async Task UpdateUser_ReturnsNotFound_WhenUserNotFound()
        {
            var dto = new UpdateUserDTO { Username = "updated" };
            _serviceMock.Setup(s => s.UpdateUser(1, dto)).ReturnsAsync((UserDTO)null);

            var result = await _controller.UpdateUser(1, dto);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Contains("not found", notFoundResult.Value.ToString());
        }

        [Fact]
        public async Task UpdateUser_ReturnsConflict_OnArgumentException()
        {
            var dto = new UpdateUserDTO { Username = "bad" };
            _serviceMock.Setup(s => s.UpdateUser(1, dto)).ThrowsAsync(new ArgumentException("Invalid"));

            var result = await _controller.UpdateUser(1, dto);
            var conflictResult = Assert.IsType<ConflictObjectResult>(result.Result);
            Assert.Contains("Invalid", conflictResult.Value.ToString());
        }

        [Fact]
        public async Task UpdateUser_ReturnsConflict_OnUnauthorizedAccessException()
        {
            var dto = new UpdateUserDTO { Username = "bad" };
            _serviceMock.Setup(s => s.UpdateUser(1, dto)).ThrowsAsync(new UnauthorizedAccessException("Nope"));

            var result = await _controller.UpdateUser(1, dto);
            var conflictResult = Assert.IsType<ConflictObjectResult>(result.Result);
            Assert.Contains("Nope", conflictResult.Value.ToString());
        }

        [Fact]
        public async Task Delete_ReturnsNoContent_WhenSuccess()
        {
            _serviceMock.Setup(s => s.DeleteUser(1)).ReturnsAsync(true);

            var result = await _controller.Delete(1);
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenUserDoesNotExist()
        {
            _serviceMock.Setup(s => s.DeleteUser(1)).ThrowsAsync(new Exception("Not found"));

            var result = await _controller.Delete(1);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains("Not found", notFoundResult.Value.ToString());
        }

        [Fact]
        public async Task GetUserCountByTaxGroup_ReturnsOkWithData()
        {
            var data = new List<object> { new { TaxGroup = "A", UserCount = 1 } };
            _serviceMock.Setup(s => s.GetUserCountByTaxGroup()).ReturnsAsync(data);

            var result = await _controller.GetUserCountByTaxGroup();
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(data, okResult.Value);
        }

        [Fact]
        public async Task GetUserCountByPlan_ReturnsOkWithData()
        {
            var data = new List<object> { new { Plan = "P1", UserCount = 2 } };
            _serviceMock.Setup(s => s.GetUserCountByPlan()).ReturnsAsync(data);

            var result = await _controller.GetUserCountByPlan();
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(data, okResult.Value);
        }
    }
}
