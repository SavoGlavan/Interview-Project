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

namespace api.Tests.Unit
{
    public class TaxGroupRepositoryTests
    {
        private readonly ApplicationDBContext _context;
        private readonly TaxGroupRepository _repository;

        public TaxGroupRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDBContext(options);
            _repository = new TaxGroupRepository(_context);
        }

        [Fact]
        public async Task CreateTaxGroup_AddsNewRecord()
        {
            var taxGroup = new TaxGroup { Name = "Standard", Vat = 10, Eco_tax = 5 };

            var created = await _repository.CreateTaxGroup(taxGroup);

            Assert.NotNull(created);
            Assert.Equal("Standard", created.Name);
            Assert.Single(_context.TaxGroups);
        }

        [Fact]
        public async Task GetAll_ReturnsAllTaxGroups()
        {
            _context.TaxGroups.AddRange(
                new TaxGroup { Name = "A", Vat = 10, Eco_tax = 5 },
                new TaxGroup { Name = "B", Vat = 20, Eco_tax = 10 }
            );
            await _context.SaveChangesAsync();

            var result = await _repository.getAll();

            Assert.Equal(2, result.Count);
            Assert.Contains(result, x => x.Name == "A");
            Assert.Contains(result, x => x.Name == "B");
        }

        [Fact]
        public async Task GetById_ReturnsCorrectTaxGroup()
        {
            var taxGroup = new TaxGroup { Name = "FindMe", Vat = 15, Eco_tax = 7 };
            _context.TaxGroups.Add(taxGroup);
            await _context.SaveChangesAsync();

            var found = await _repository.GetById(taxGroup.Id);

            Assert.NotNull(found);
            Assert.Equal("FindMe", found.Name);
        }

        [Fact]
        public async Task UpdateTaxGroup_ChangesValues()
        {
            var taxGroup = new TaxGroup { Name = "Old", Vat = 10, Eco_tax = 5 };
            _context.TaxGroups.Add(taxGroup);
            await _context.SaveChangesAsync();

            var dto = new UpdateTaxGroupDTO { Name = "Updated", Vat = 25, Eco_tax = 15 };

            var updated = await _repository.UpdateTaxGroup(taxGroup.Id, dto);

            Assert.NotNull(updated);
            Assert.Equal("Updated", updated.Name);
            Assert.Equal(25, updated.Vat);
            Assert.Equal(15, updated.Eco_tax);
        }

        [Fact]
        public async Task UpdateTaxGroup_ReturnsNull_IfNotFound()
        {
            var dto = new UpdateTaxGroupDTO { Name = "None" };

            var result = await _repository.UpdateTaxGroup(999, dto);

            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteTaxGroup_RemovesRecord()
        {
            var taxGroup = new TaxGroup { Name = "ToDelete", Vat = 8, Eco_tax = 4 };
            _context.TaxGroups.Add(taxGroup);
            await _context.SaveChangesAsync();

            var success = await _repository.DeleteTaxGroup(taxGroup.Id);

            Assert.True(success);
            Assert.Empty(_context.TaxGroups);
        }

        [Fact]
        public async Task DeleteTaxGroup_ReturnsFalse_IfNotFound()
        {
            var success = await _repository.DeleteTaxGroup(1234);
            Assert.False(success);
        }
    }
}
