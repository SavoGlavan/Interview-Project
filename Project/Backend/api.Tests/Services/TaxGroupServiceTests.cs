using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using api.DTO;
using api.Interfaces;
using api.Model;
using api.Repositories;
using api.Services;
using Moq;
using Xunit;

namespace api.Tests.Unit
{
    public class TaxGroupServiceTests
    {
        private readonly Mock<ITaxGroupRepository> _repoMock;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly TaxGroupService _service;

        public TaxGroupServiceTests()
        {
            _repoMock = new Mock<ITaxGroupRepository>();
            _userRepoMock = new Mock<IUserRepository>();
            _service = new TaxGroupService(_repoMock.Object, _userRepoMock.Object);
        }

        [Fact]
        public async Task GetAllTaxGroups_ReturnsMappedList()
        {
            var taxGroups = new List<TaxGroup>
            {
                new TaxGroup { Id = 1, Name = "Standard", Vat = 10, Eco_tax = 5 },
                new TaxGroup { Id = 2, Name = "Premium", Vat = 20, Eco_tax = 10 }
            };
            _repoMock.Setup(r => r.getAll()).ReturnsAsync(taxGroups);

            var result = await _service.getAllTaxGroups();

            Assert.Equal(2, result.Count);
            Assert.Contains(result, t => t.Name == "Standard");
            Assert.Contains(result, t => t.Name == "Premium");
        }

        [Fact]
        public async Task GetTaxGroupById_ReturnsCorrectDTO()
        {
            var taxGroup = new TaxGroup { Id = 1, Name = "Test", Vat = 15, Eco_tax = 7 };
            _repoMock.Setup(r => r.GetById(1)).ReturnsAsync(taxGroup);

            var result = await _service.getTaxGroupById(1);

            Assert.NotNull(result);
            Assert.Equal("Test", result.Name);
            Assert.Equal(15, result.Vat);
            Assert.Equal(7, result.EcoTax);
        }

        [Fact]
        public async Task GetTaxGroupById_ReturnsNull_WhenNotFound()
        {
            _repoMock.Setup(r => r.GetById(1)).ReturnsAsync((TaxGroup)null);

            var result = await _service.getTaxGroupById(1);

            Assert.Null(result);
        }

        [Fact]
        public async Task CreateTaxGroup_SuccessfullyCreates()
        {
            var dto = new CreateTaxGroupDTO { Name = "New", Vat = 10, Eco_tax = 5 };
            var created = new TaxGroup { Id = 1, Name = "New", Vat = 10, Eco_tax = 5 };

            _repoMock.Setup(r => r.CreateTaxGroup(It.IsAny<TaxGroup>())).ReturnsAsync(created);

            var result = await _service.CreateTaxGroup(dto);

            Assert.Equal("New", result.Name);
            Assert.Equal(10, result.Vat);
            Assert.Equal(5, result.EcoTax);
        }

        [Theory]
        [InlineData("", 10, 5, "Plan name cannot be empty.")]
        [InlineData("X", 0, 5, "VAT must be a positive number.")]
        [InlineData("X", 10, 0, "Eco tax must be a positive number.")]
        public async Task CreateTaxGroup_Throws_OnInvalidData(string name, decimal vat, decimal ecoTax, string expectedMsg)
        {
            var dto = new CreateTaxGroupDTO { Name = name, Vat = vat, Eco_tax = ecoTax };

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateTaxGroup(dto));
            Assert.Contains(expectedMsg, ex.Message);
        }

       [Fact]
        public async Task UpdateTaxGroup_SuccessfullyUpdates()
        {
            var dto = new UpdateTaxGroupDTO { Name = "Updated", Vat = 25, Eco_tax = 15 };
            var taxGroup = new TaxGroup { Id = 1, Name = "Old", Vat = 10, Eco_tax = 5 };

            var updatedTaxGroup = new TaxGroup
            {
                Id = taxGroup.Id,
                Name = "Updated",
                Vat = 25,
                Eco_tax = 15
            };

            _repoMock.Setup(r => r.UpdateTaxGroup(1, dto)).ReturnsAsync(updatedTaxGroup);

            var result = await _service.UpdateTaxGroup(1, dto);

            Assert.NotNull(result);
            Assert.Equal("Updated", result.Name);
            Assert.Equal(25, result.Vat);
            Assert.Equal(15, result.EcoTax);
        }


        [Theory]
        [InlineData("", 10, 5, "Tax group name cannot be empty.")]
        [InlineData("X", 0, 5, "VAT must be a positive number.")]
        [InlineData("X", 10, 0, "Eco tax must be a positive number.")]
        public async Task UpdateTaxGroup_Throws_OnInvalidData(string name, decimal vat, decimal ecoTax, string expectedMsg)
        {
            var dto = new UpdateTaxGroupDTO { Name = name, Vat = vat, Eco_tax = ecoTax };

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateTaxGroup(1, dto));
            Assert.Contains(expectedMsg, ex.Message);
        }

        [Fact]
        public async Task UpdateTaxGroup_ReturnsNull_WhenNotFound()
        {
            _repoMock.Setup(r => r.UpdateTaxGroup(1, It.IsAny<UpdateTaxGroupDTO>()))
                .ReturnsAsync((TaxGroup)null);

            var dto = new UpdateTaxGroupDTO { Name = "X", Vat = 10, Eco_tax = 5 };
            var result = await _service.UpdateTaxGroup(1, dto);

            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteTaxGroup_SuccessfullyDeletes()
        {
            var taxGroup = new TaxGroup { Id = 1, Name = "ToDelete" };
            _repoMock.Setup(r => r.GetById(1)).ReturnsAsync(taxGroup);
            _userRepoMock.Setup(u => u.taxGroupHasUsers(1)).ReturnsAsync(false);
            _repoMock.Setup(r => r.DeleteTaxGroup(1)).ReturnsAsync(true);

            var result = await _service.DeleteTaxGroup(1);

            Assert.True(result);
            _repoMock.Verify(r => r.DeleteTaxGroup(1), Times.Once);
        }

        [Fact]
        public async Task DeleteTaxGroup_Throws_WhenNotFound()
        {
            _repoMock.Setup(r => r.GetById(1)).ReturnsAsync((TaxGroup)null);

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.DeleteTaxGroup(1));
            Assert.Contains("not found", ex.Message);
        }

        [Fact]
        public async Task DeleteTaxGroup_Throws_WhenInUse()
        {
            var taxGroup = new TaxGroup { Id = 1, Name = "InUse" };
            _repoMock.Setup(r => r.GetById(1)).ReturnsAsync(taxGroup);
            _userRepoMock.Setup(u => u.taxGroupHasUsers(1)).ReturnsAsync(true);

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.DeleteTaxGroup(1));
            Assert.Contains("Cannot delete", ex.Message);
        }
    }
}
