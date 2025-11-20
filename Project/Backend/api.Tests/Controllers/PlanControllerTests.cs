using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using api.Controllers;
using api.DTO;
using api.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace api.Tests.Controllers
{
    public class PlanControllerTests
    {
        private readonly Mock<IPlanService> _mockService;
        private readonly PlanController _controller;

        public PlanControllerTests()
        {
            _mockService = new Mock<IPlanService>();
            _controller = new PlanController(_mockService.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOkResult_WithListOfPlans()
        {
            // Arrange
            var plans = new List<PlanDTO>
            {
                new PlanDTO 
                { 
                    Id = 1, 
                    Name = "Standard", 
                    Discount = 0, 
                    Prices = new List<PriceThresholdDTO>() 
                },
                new PlanDTO 
                { 
                    Id = 2, 
                    Name = "Premium", 
                    Discount = 10, 
                    Prices = new List<PriceThresholdDTO>() 
                }
            };
            _mockService.Setup(s => s.getAllPlans()).ReturnsAsync(plans);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedPlans = Assert.IsAssignableFrom<List<PlanDTO>>(okResult.Value);
            Assert.Equal(2, returnedPlans.Count);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult_WhenPlanExists()
        {
            // Arrange
            var plan = new PlanDTO 
            { 
                Id = 1, 
                Name = "Standard", 
                Discount = 0, 
                Prices = new List<PriceThresholdDTO>() 
            };
            _mockService.Setup(s => s.getPlanById(1)).ReturnsAsync(plan);

            // Act
            var result = await _controller.GetById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedPlan = Assert.IsType<PlanDTO>(okResult.Value);
            Assert.Equal(1, returnedPlan.Id);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenPlanDoesNotExist()
        {
            // Arrange
            _mockService.Setup(s => s.getPlanById(1)).ReturnsAsync((PlanDTO)null);

            // Act
            var result = await _controller.GetById(1);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Create_ReturnsCreatedAtAction_WhenPlanIsValid()
        {
            // Arrange
            var dto = new CreatePlanDTO 
            { 
                Name = "Standard", 
                Discount = 0, 
                Prices = new List<CreatePriceThresholdDTO>() 
            };
            var createdPlan = new PlanDTO 
            { 
                Id = 1, 
                Name = "Standard", 
                Discount = 0, 
                Prices = new List<PriceThresholdDTO>() 
            };
            _mockService.Setup(s => s.CreatePlan(dto)).ReturnsAsync(createdPlan);

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnedPlan = Assert.IsType<PlanDTO>(createdResult.Value);
            Assert.Equal(1, returnedPlan.Id);
        }

        [Fact]
        public async Task Delete_ReturnsNoContent_WhenSuccessful()
        {
            // Arrange
            _mockService.Setup(s => s.DeletePlan(1)).ReturnsAsync(true);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsConflict_WhenPlanInUse()
        {
            // Arrange
            _mockService.Setup(s => s.DeletePlan(1)).ThrowsAsync(new ArgumentException("Plan in use"));

            // Act
            var result = await _controller.Delete(1);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Contains("Plan in use", conflictResult.Value.ToString());
        }
    }
}
