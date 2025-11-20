using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.DTO;
using api.Model;
using api.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace api.Tests.Repositories
{
    public class PlanRepositoryTests
    {
        private readonly ApplicationDBContext _context;
        private readonly PlanRepository _repository;

        public PlanRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // fresh DB for each test
                .Options;

            _context = new ApplicationDBContext(options);
            _repository = new PlanRepository(_context);
        }

        [Fact]
        public async Task CreatePlan_ShouldAddPlanToDatabase()
        {
            var plan = new Plan
            {
                Name = "Test Plan",
                Discount = 5,
                Prices = new List<PriceThreshold>
                {
                    new() { Price = 0.1m, Threshold = 100 },
                    new() { Price = 0.08m, Threshold = null }
                }
            };

            var result = await _repository.CreatePlan(plan);

            var saved = await _context.Plans.Include(p => p.Prices).FirstOrDefaultAsync();
            Assert.NotNull(saved);
            Assert.Equal("Test Plan", saved.Name);
            Assert.Equal(2, saved.Prices.Count);
        }

        [Fact]
        public async Task GetById_ShouldReturnPlan_WhenExists()
        {
            var plan = new Plan
            {
                Name = "Standard",
                Discount = 10,
                Prices = new List<PriceThreshold> { new() { Price = 0.1m, Threshold = null } }
            };
            _context.Plans.Add(plan);
            await _context.SaveChangesAsync();

            var result = await _repository.GetById(plan.Id);

            Assert.NotNull(result);
            Assert.Equal("Standard", result.Name);
        }

        [Fact]
        public async Task GetById_ShouldReturnNull_WhenNotExists()
        {
            var result = await _repository.GetById(999);
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdatePlan_ShouldModifyExistingPlan()
        {
            var plan = new Plan
            {
                Name = "Old Plan",
                Discount = 5,
                Prices = new List<PriceThreshold> { new() { Price = 0.1m, Threshold = 100 } }
            };
            _context.Plans.Add(plan);
            await _context.SaveChangesAsync();

            var dto = new UpdatePlanDTO
            {
                Name = "Updated Plan",
                Discount = 15,
                Prices = new List<UpdatePriceThresholdDTO>
                {
                    new() { Id = plan.Prices.First().Id, Price = 0.12m, Threshold = 200 },
                    new() { Price = 0.08m, Threshold = null } // new open-ended tier
                }
            };

            var updated = await _repository.UpdatePlan(plan.Id, dto);

            Assert.NotNull(updated);
            Assert.Equal("Updated Plan", updated.Name);
            Assert.Equal(15, updated.Discount);
            Assert.Equal(2, updated.Prices.Count);
            Assert.Contains(updated.Prices, p => p.Threshold == null);
        }

        [Fact]
        public async Task UpdatePlan_ShouldReturnNull_WhenPlanNotFound()
        {
            var dto = new UpdatePlanDTO
            {
                Name = "NonExistent",
                Prices = new List<UpdatePriceThresholdDTO>()
            };

            var result = await _repository.UpdatePlan(999, dto);
            Assert.Null(result);
        }

        [Fact]
        public async Task DeletePlan_ShouldRemovePlan_WhenExists()
        {
            var plan = new Plan
            {
                Name = "DeleteMe",
                Discount = 0,
                Prices = new List<PriceThreshold> { new() { Price = 0.1m, Threshold = null } }
            };
            _context.Plans.Add(plan);
            await _context.SaveChangesAsync();

            var success = await _repository.DeletePlan(plan.Id);

            Assert.True(success);
            Assert.False(_context.Plans.Any());
            Assert.False(_context.PriceThresholds.Any());
        }

        [Fact]
        public async Task DeletePlan_ShouldReturnFalse_WhenNotFound()
        {
            var success = await _repository.DeletePlan(999);
            Assert.False(success);
        }
    }
}
