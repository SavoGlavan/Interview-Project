using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.DTO;
using api.Model;
using api.Repositories;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace api.Tests.Unit
{
    public class UserRepositoryTests
    {
        private readonly ApplicationDBContext _context;
        private readonly UserRepository _repository;

       

        public UserRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDBContext(options);
            _repository = new UserRepository(_context);
           
        }

        private async Task<User> AddSampleUser(string username = "user1")
        {
            var taxGroup = new TaxGroup { Name = "Standard", Vat = 10, Eco_tax = 5 };
            var plan = new Plan { Name = "Basic", Discount = 10 };
            
            _context.Plans.Add(plan);
            await _context.SaveChangesAsync();

            var user = new User
            {
                Username = username,
                Password = "password",
                Role = "user",
                Consumption = 100,
                TaxGroupId = taxGroup.Id,
                PlanId = plan.Id,
                Email = $"{username}@example.com"
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        [Fact]
        public async Task GetAll_ReturnsAllUsers()
        {
            await AddSampleUser("user1");
            await AddSampleUser("user2");

            var users = await _repository.getAll();
            Assert.Equal(2, users.Count);
            Assert.Contains(users, u => u.Username == "user1");
            Assert.Contains(users, u => u.Username == "user2");
        }

        [Fact]
        public async Task GetById_ReturnsCorrectUser()
        {
            var user = await AddSampleUser();
            var result = await _repository.GetById(user.Id);
            Assert.NotNull(result);
            Assert.Equal(user.Username, result.Username);
        }

        [Fact]
        public async Task GetByUsername_ReturnsCorrectUser()
        {
            var user = await AddSampleUser();
            var result = await _repository.GetByUsername(user.Username);
            Assert.NotNull(result);
            Assert.Equal(user.Id, result.Id);
        }

        [Fact]
        public async Task CreateUser_AddsUserSuccessfully()
        {
            var taxGroup = new TaxGroup { Name = "Standard", Vat = 10, Eco_tax = 5 };
            var plan = new Plan { Name = "Basic", Discount = 10 };
            _context.TaxGroups.Add(taxGroup);
            _context.Plans.Add(plan);
            await _context.SaveChangesAsync();

            var user = new User
            {
                Username = "newUser",
                Password = "password",
                Role = "user",
                Consumption = 100,
                TaxGroupId = taxGroup.Id,
                PlanId = plan.Id,
                Email = "newUser@example.com"
            };

            var created = await _repository.CreateUser(user);

            Assert.NotNull(created);
            Assert.Equal("newUser", created.Username);
            Assert.NotNull(created.TaxGroup);
            Assert.NotNull(created.Plan);
        }

        [Fact]
        public async Task UpdateUser_ChangesProperties()
        {
            var user = await AddSampleUser();

            var updateDto = new UpdateUserDTO
            {
                Username = "updatedUser",
                Password = "newpass",
                Consumption = 200
            };

            var updated = await _repository.UpdateUser(user.Id, updateDto);
            Assert.NotNull(updated);
            Assert.Equal("updatedUser", updated.Username);
            Assert.Equal("newpass", updated.Password);
            Assert.Equal(200, updated.Consumption);
        }

        [Fact]
        public async Task DeleteUser_RemovesUser()
        {
            var user = await AddSampleUser();
            var result = await _repository.DeleteUser(user.Id);
            Assert.True(result);

            var deleted = await _repository.GetById(user.Id);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task PlanHasUsers_ReturnsTrueIfUsersExist()
        {
            var user = await AddSampleUser();
            var result = await _repository.planHasUsers(user.PlanId.Value);
            Assert.True(result);
        }

        [Fact]
        public async Task TaxGroupHasUsers_ReturnsTrueIfUsersExist()
        {
            var user = await AddSampleUser();
            var result = await _repository.taxGroupHasUsers(user.TaxGroupId.Value);
            Assert.True(result);
        }

    }
}
