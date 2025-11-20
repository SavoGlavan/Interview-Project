using System;
using System.Collections.Generic;
using System.Linq;
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
    public class PlanIntegrationTests
    {
        private readonly ApplicationDBContext _context;
        private readonly PlanRepository _repository;
        private readonly PlanService _service;
        private readonly PlanController _controller;

        private readonly TaxGroupRepository _taxGroupRepository;

        private readonly UserRepository _userRepository;
        public PlanIntegrationTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDBContext(options);
            _repository = new PlanRepository(_context);
            _taxGroupRepository=new TaxGroupRepository(_context);
            _userRepository=new UserRepository(_context);
            // Pass null for unused repositories
            _service = new PlanService(_repository, _taxGroupRepository, _userRepository);
            _controller = new PlanController(_service);
        }

        private CreatePlanDTO GetSamplePlanDto(string name = "Standard", decimal discount = 10, decimal price1 = 0.10M, decimal price2 = 0.08M)
        {
            return new CreatePlanDTO
            {
                Name = name,
                Discount = discount,
                Prices = new List<CreatePriceThresholdDTO>
                {
                    new CreatePriceThresholdDTO { Price = price1, Threshold = 100 },
                    new CreatePriceThresholdDTO { Price = price2, Threshold = null } // open-ended
                }
            };
        }

        [Fact]
        public async Task CreatePlan_ThenGetById_ReturnsSamePlan()
        {
            var dto = GetSamplePlanDto();

            var actionResult = await _controller.Create(dto);
            var createResult = actionResult.Result as CreatedAtActionResult;
            var createdPlan = createResult.Value as PlanDTO;

            var getActionResult = await _controller.GetById(createdPlan.Id);
            var getResult = getActionResult.Result as OkObjectResult;
            var retrievedPlan = getResult.Value as PlanDTO;

            Assert.Equal(createdPlan.Id, retrievedPlan.Id);
            Assert.Equal(dto.Name, retrievedPlan.Name);
            Assert.Equal(dto.Prices.Count, retrievedPlan.Prices.Count);
        }

        [Fact]
        public async Task GetAllPlans_ReturnsCreatedPlans()
        {
            var dto1 = GetSamplePlanDto("PlanA");
            var dto2 = GetSamplePlanDto("PlanB");

            await _controller.Create(dto1);
            await _controller.Create(dto2);

            var actionResult = await _controller.GetAll();
            var okResult = actionResult.Result as OkObjectResult;
            var plans = okResult.Value as List<PlanDTO>;

            Assert.True(plans.Count >= 2);
            Assert.Contains(plans, p => p.Name == "PlanA");
            Assert.Contains(plans, p => p.Name == "PlanB");
        }

        [Fact]
        public async Task UpdatePlan_ChangesPlanValues()
        {
            var dto = GetSamplePlanDto();
            var createResult = await _controller.Create(dto);
            var createdPlan = (createResult.Result as CreatedAtActionResult)?.Value as PlanDTO;

            var updateDto = new UpdatePlanDTO
            {
                Name = "UpdatedPlan",
                Discount = 15,
                Prices = new List<UpdatePriceThresholdDTO>
                {
                    new UpdatePriceThresholdDTO { Id = createdPlan.Prices[0].Id, Price = 0.12M, Threshold = 100 },
                    new UpdatePriceThresholdDTO { Id = createdPlan.Prices[1].Id, Price = 0.09M, Threshold = null }
                }
            };

            var updateActionResult = await _controller.UpdatePlan(createdPlan.Id, updateDto);
            var updateResult = updateActionResult.Result as OkObjectResult;
            var updatedPlan = updateResult.Value as PlanDTO;

            Assert.Equal("UpdatedPlan", updatedPlan.Name);
            Assert.Equal(15, updatedPlan.Discount);
            Assert.Equal(0.12M, updatedPlan.Prices[0].Price);
        }

        [Fact]
        public async Task RecommendPlan_ReturnsBestPlan()
        {
            // Add a tax group
            var taxGroup = new TaxGroup
            {
                Name = "Standard",
                Vat = 10,
                Eco_tax = 5
            };
           

            var createdTaxGroup=await _taxGroupRepository.CreateTaxGroup(taxGroup);
            // Plans with different prices (cheapPlan is actually cheaper)
            var cheapPlan = GetSamplePlanDto("CheapPlan", 0.10M, 0.05M, 0.03M);
            var expensivePlan = GetSamplePlanDto("ExpensivePlan", 0.10M, 0.10M, 0.08M);

            await _controller.Create(cheapPlan);
            await _controller.Create(expensivePlan);

            var request = new RecommendationRequestDTO
            {
                Consumption = 150,
                TaxGroupId = createdTaxGroup.Id
            };

            var actionResult = await _controller.RecommendPlan(request);
            var okResult = actionResult.Result as OkObjectResult;
            var recommendation = okResult.Value as RecomendationResponseDTO;

            Assert.NotNull(recommendation);
            Assert.Equal("CheapPlan", recommendation.planDTO.Name);
            Assert.True(recommendation.totalPrice > 0);
        }

        [Fact]
        public async Task CreatePlan_InvalidData_ThrowsArgumentException()
        {
            var dto = new CreatePlanDTO
            {
                Name = "",
                Discount = -10,
                Prices = new List<CreatePriceThresholdDTO>()
            };

            var actionResult = await _controller.Create(dto);
            var result = actionResult.Result as ObjectResult;
            Assert.Equal(409, result.StatusCode); // Conflict
        }

        [Fact]
        public async Task DeletePlan_RemovesPlan()
        {
            var dto = GetSamplePlanDto("ToDelete");
            var createResult = await _controller.Create(dto);
            var createdPlan = (createResult.Result as CreatedAtActionResult)?.Value as PlanDTO;

            var deleteResult = await _controller.Delete(createdPlan.Id);

            // Controller now returns NoContentResult on success
            Assert.IsType<NoContentResult>(deleteResult);

            var getResult = await _controller.GetById(createdPlan.Id);
            Assert.IsType<NotFoundResult>(getResult.Result);
        }
    }
}
