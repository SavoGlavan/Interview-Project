using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using api.Controllers;
using api.Data;
using api.DTO;
using api.Model;
using api.Repositories;
using api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace api.Tests.Integration
{
    public class UserIntegrationTests
    {
        private readonly ApplicationDBContext _context;
        private readonly UserRepository _userRepository;
        private readonly TaxGroupRepository _taxGroupRepository;
        private readonly PlanRepository _planRepository;
        private readonly UserService _service;
        private readonly UserController _controller;

        public UserIntegrationTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDBContext(options);

            // Initialize repositories
            _userRepository = new UserRepository(_context);
            _taxGroupRepository = new TaxGroupRepository(_context);
            _planRepository = new PlanRepository(_context);

            // Mock HttpContext for UserService (needed for UpdateUser authorization)
            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"), // logged-in user id
            }));

            var httpContextAccessor = new HttpContextAccessor { HttpContext = httpContext };

            _service = new UserService(_userRepository, httpContextAccessor, _taxGroupRepository, _planRepository);
            _controller = new UserController(_service);
        }

        private TaxGroup CreateSampleTaxGroup(string name = "Standard")
        {
            var taxGroup = new TaxGroup { Name = name, Vat = 10, Eco_tax = 5 };
            _context.TaxGroups.Add(taxGroup);
            _context.SaveChanges();
            return taxGroup;
        }

        private Plan CreateSamplePlan(string name = "Standard")
        {
            var plan = new Plan { Name = name, Discount = 10 };
            _context.Plans.Add(plan);
            _context.SaveChanges();
            return plan;
        }

        private CreateUserDTO GetSampleUserDto(int? taxGroupId = null, int? planId = null)
        {
            return new CreateUserDTO
            {
                Username = "user1",
                Password = "pass",
                Consumption = 100,
                Role = "user",
                Email = "user@example.com",
                TaxGroupId = taxGroupId,
                PlanId = planId
            };
        }

        [Fact]
        public async Task CreateUser_ThenGetById_ReturnsSameUser()
        {
            var taxGroup = CreateSampleTaxGroup();
            var plan = CreateSamplePlan();

            var dto = GetSampleUserDto(taxGroup.Id, plan.Id);

            var createdUser = await _service.CreateUser(dto);
            

            var getResult = await _controller.GetById(createdUser.Id);
            var okResult = getResult.Result as OkObjectResult;
            var retrievedUser = okResult.Value as UserDTO;

            Assert.Equal(createdUser.Id, retrievedUser.Id);
            Assert.Equal(dto.Username, retrievedUser.Username);
            Assert.Equal(dto.Email, retrievedUser.Email);
            Assert.Equal(taxGroup.Name, retrievedUser.Details.TaxGroupName);
        }

        [Fact]
        public async Task GetAll_ReturnsCreatedUsers()
        {
            var taxGroup = CreateSampleTaxGroup();
            var plan = CreateSamplePlan();

            var user1 = GetSampleUserDto(taxGroup.Id, plan.Id);
            user1.Username = "user1";
            var user2 = GetSampleUserDto(taxGroup.Id, plan.Id);
            user2.Username = "user2";

            await _service.CreateUser(user1);
            await _service.CreateUser(user2);

            var result = await _controller.GetAll();
            var okResult = result.Result as OkObjectResult;
            var users = okResult.Value as List<UserDTO>;

            Assert.True(users.Count >= 2);
            Assert.Contains(users, u => u.Username == "user1");
            Assert.Contains(users, u => u.Username == "user2");
        }

        [Fact]
        public async Task UpdateUser_ChangesValues()
        {
            var taxGroup = CreateSampleTaxGroup();
            var plan = CreateSamplePlan();

            var dto = GetSampleUserDto(taxGroup.Id, plan.Id);
            var createdUser = await _service.CreateUser(dto);
            

            // Update username and consumption
            var updateDto = new UpdateUserDTO
            {
                Username = "updatedUser",
                Consumption = 200
            };

            var updateResult = await _controller.UpdateUser(createdUser.Id, updateDto);
            var okResult = updateResult.Result as OkObjectResult;
            var updatedUser = okResult.Value as UserDTO;

            Assert.Equal("updatedUser", updatedUser.Username);
            Assert.Equal(200, updatedUser.Consumption);
        }

        

     

    }
}
