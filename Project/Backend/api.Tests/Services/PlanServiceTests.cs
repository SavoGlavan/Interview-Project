using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.DTO;
using api.Interfaces;
using api.Model;
using api.Repositories;
using api.Services;
using Moq;
using Xunit;

namespace api.Tests
{
    public class PlanServiceTests
    {
        private readonly Mock<IPlanRepository> _planRepoMock;
        private readonly Mock<ITaxGroupRepository> _taxRepoMock;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly PlanService _service;

        public PlanServiceTests()
        {
            _planRepoMock = new Mock<IPlanRepository>();
            _taxRepoMock = new Mock<ITaxGroupRepository>();
            _userRepoMock = new Mock<IUserRepository>();

            _service = new PlanService(
                _planRepoMock.Object,
                _taxRepoMock.Object,
                _userRepoMock.Object
            );
        }

        [Fact]
        public void CalculateTotalCost_ShouldReturnCorrectValue()
        {
            var plan = new Plan
            {   
                Name="Test Plan",
                Discount = 10,
                Prices = new List<PriceThreshold>
                {
                    new() { Threshold = 100, Price = 0.10m },
                    new() { Threshold = null, Price = 0.08m }
                }
            };

            var tax = new TaxGroup { Vat = 10, Eco_tax = 5, Name="Test1" };

            var result = _service.CalculateTotalCost(plan, 200, tax);
            Assert.Equal(18.63m, result, 2); // 2 decimal places precision
        }

        [Fact]
        public async Task RecommendPlan_ShouldReturnBestPlan()
        {
            var tax = new TaxGroup { Vat = 10, Eco_tax = 5, Name="Test2" };
            _taxRepoMock.Setup(r => r.GetById(It.IsAny<int>())).ReturnsAsync(tax);

            var plans = new List<Plan>
            {
                new() { Id = 1, Name = "A", Discount = 0, Prices = new List<PriceThreshold> { new() { Price = 0.10m, Threshold = null } } },
                new() { Id = 2, Name = "B", Discount = 0, Prices = new List<PriceThreshold> { new() { Price = 0.05m, Threshold = null } } }
            };

            _planRepoMock.Setup(r => r.getAll()).ReturnsAsync(plans);

            var result = await _service.RecommendPlan(1000, 1);

            Assert.NotNull(result);
            Assert.Equal("B", result.planDTO.Name);
            Assert.True(result.totalPrice > 0);
        }

        [Fact]
        public async Task CreatePlan_ShouldThrowException_WhenDuplicateThresholds()
        {
            var dto = new CreatePlanDTO
            {
                Name = "Test Plan",
                Discount = 5,
                Prices = new List<CreatePriceThresholdDTO>
                {
                    new() { Price = 0.10m, Threshold = 100 },
                    new() { Price = 0.15m, Threshold = 100 }
                }
            };

            await Assert.ThrowsAsync<ArgumentException>(() => _service.CreatePlan(dto));
        }

        [Fact]
        public async Task CreatePlan_ShouldThrowException_WhenNoOpenEndedTier()
        {
            var dto = new CreatePlanDTO
            {
                Name = "Test Plan",
                Discount = 10,
                Prices = new List<CreatePriceThresholdDTO>
                {
                    new() { Price = 0.10m, Threshold = 100 },
                    new() { Price = 0.08m, Threshold = 300 }
                }
            };

            await Assert.ThrowsAsync<ArgumentException>(() => _service.CreatePlan(dto));
        }

        [Fact]
        public async Task DeletePlan_ShouldThrow_WhenPlanNotFound()
        {
            _planRepoMock.Setup(r => r.GetById(It.IsAny<int>())).ReturnsAsync((Plan?)null);

            await Assert.ThrowsAsync<Exception>(() => _service.DeletePlan(1));
        }

        [Fact]
        public async Task DeletePlan_ShouldThrow_WhenPlanHasUsers()
        {
            var planId = 1;

            _planRepoMock.Setup(r => r.GetById(It.IsAny<int>())).ReturnsAsync(new Plan
            {
                Id = planId,
                Name = "Test Plan",
                Discount = 0,
                Prices = new List<PriceThreshold>
                {
                    new PriceThreshold { Id = 1, Price = 0.1m, Threshold = 100 }
                }
            });

            _userRepoMock.Setup(r => r.planHasUsers(planId)).ReturnsAsync(true);

            await Assert.ThrowsAsync<ArgumentException>(() => _service.DeletePlan(planId));
        }

        [Fact]
        public async Task GetPlanById_ShouldReturnNull_WhenPlanMissing()
        {
            _planRepoMock.Setup(r => r.GetById(5)).ReturnsAsync((Plan?)null);

            var result = await _service.getPlanById(5);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllPlans_ShouldReturnSortedTiers()
        {
            var plan = new Plan
            {
                Id = 1,
                Name = "Test",
                Prices = new List<PriceThreshold>
                {
                    new() { Id = 2, Threshold = 300, Price = 0.09m },
                    new() { Id = 1, Threshold = 100, Price = 0.10m },
                    new() { Id = 3, Threshold = null, Price = 0.08m }
                }
            };

            _planRepoMock.Setup(r => r.getAll()).ReturnsAsync(new List<Plan> { plan });

            var result = await _service.getAllPlans();

            Assert.Equal(3, result.First().Prices.Count);
            Assert.True(result.First().Prices[0].Threshold < result.First().Prices[1].Threshold);
        }
    }
}
