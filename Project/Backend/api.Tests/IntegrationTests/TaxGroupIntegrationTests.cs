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
using Xunit;

namespace api.Tests.Integration
{
    public class TaxGroupIntegrationTests
    {
        private readonly ApplicationDBContext _context;
        private readonly TaxGroupRepository _repository;
        private readonly UserRepository _userRepository;
        private readonly TaxGroupService _service;
        private readonly TaxGroupController _controller;

        public TaxGroupIntegrationTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDBContext(options);
            _repository = new TaxGroupRepository(_context);
            _userRepository = new UserRepository(_context);
            _service = new TaxGroupService(_repository, _userRepository);
            _controller = new TaxGroupController(_service);
        }

        private CreateTaxGroupDTO GetSampleDto(string name = "Standard", decimal vat = 10, decimal ecoTax = 5)
        {
            return new CreateTaxGroupDTO
            {
                Name = name,
                Vat = vat,
                Eco_tax = ecoTax
            };
        }

        [Fact]
        public async Task CreateTaxGroup_ThenGetById_ReturnsSameGroup()
        {
            var dto = GetSampleDto("Basic", 10, 5);

            var createAction = await _controller.Create(dto);
            var createResult = createAction.Result as CreatedAtActionResult;
            var created = createResult.Value as TaxGroupDTO;

            var getAction = await _controller.GetById(created.Id);
            var okResult = getAction.Result as OkObjectResult;
            var retrieved = okResult.Value as TaxGroupDTO;

            Assert.Equal(created.Id, retrieved.Id);
            Assert.Equal("Basic", retrieved.Name);
            Assert.Equal(10, retrieved.Vat);
        }

        [Fact]
        public async Task GetAll_ReturnsAllTaxGroups()
        {
            await _controller.Create(GetSampleDto("Group1"));
            await _controller.Create(GetSampleDto("Group2"));

            var actionResult = await _controller.GetAll();
            var okResult = actionResult.Result as OkObjectResult;
            var list = okResult.Value as List<TaxGroupDTO>;

            Assert.True(list.Count >= 2);
            Assert.Contains(list, g => g.Name == "Group1");
            Assert.Contains(list, g => g.Name == "Group2");
        }

        [Fact]
        public async Task UpdateTaxGroup_UpdatesValuesCorrectly()
        {
            var create = await _controller.Create(GetSampleDto("Old", 10, 5));
            var created = (create.Result as CreatedAtActionResult)?.Value as TaxGroupDTO;

            var updateDto = new UpdateTaxGroupDTO
            {
                Name = "Updated",
                Vat = 20,
                Eco_tax = 10
            };

            var updateAction = await _controller.UpdateTaxGroup(created.Id, updateDto);
            var okResult = updateAction.Result as OkObjectResult;
            var updated = okResult.Value as TaxGroupDTO;

            Assert.Equal("Updated", updated.Name);
            Assert.Equal(20, updated.Vat);
            Assert.Equal(10, updated.EcoTax);
        }

        [Fact]
        public async Task DeleteTaxGroup_RemovesRecord()
        {
            var create = await _controller.Create(GetSampleDto("ToDelete"));
            var created = (create.Result as CreatedAtActionResult)?.Value as TaxGroupDTO;

            var deleteResult = await _controller.Delete(created.Id);

            Assert.IsType<NoContentResult>(deleteResult);

            var getAction = await _controller.GetById(created.Id);
            Assert.IsType<NotFoundResult>(getAction.Result);
        }

        [Fact]
        public async Task CreateTaxGroup_InvalidData_ReturnsConflict()
        {
            var dto = new CreateTaxGroupDTO
            {
                Name = "",
                Vat = 0,
                Eco_tax = -1
            };

            var actionResult = await _controller.Create(dto);
            var result = actionResult.Result as ObjectResult;

            Assert.Equal(409, result.StatusCode);
        }

        [Fact]
        public async Task DeleteTaxGroup_InUse_ReturnsConflict()
        {
            // Create a tax group
            var createAction = await _controller.Create(GetSampleDto("InUse"));
            var createdGroup = (createAction.Result as CreatedAtActionResult)?.Value as TaxGroupDTO;

            // Create a user assigned to it
            var user = new User
            {
                Username = "TestUser",
                Password = "123",
                Role = "user",
                TaxGroupId = createdGroup.Id
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var result = await _controller.Delete(createdGroup.Id);
            var objectResult = result as ObjectResult;

            Assert.Equal(409, objectResult.StatusCode);
        }
    }
}
