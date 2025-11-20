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
    public class TaxGroupControllerTests
    {
        private readonly Mock<ITaxGroupService> _serviceMock;
        private readonly TaxGroupController _controller;

        public TaxGroupControllerTests()
        {
            _serviceMock = new Mock<ITaxGroupService>();
            _controller = new TaxGroupController(_serviceMock.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOk_WithListOfTaxGroups()
        {
            var list = new List<TaxGroupDTO>
            {
                new TaxGroupDTO { Id = 1, Name = "Standard", Vat = 10, EcoTax = 5 },
                new TaxGroupDTO { Id = 2, Name = "Premium", Vat = 20, EcoTax = 10 }
            };

            _serviceMock.Setup(s => s.getAllTaxGroups()).ReturnsAsync(list);

            var actionResult = await _controller.GetAll();
            var okResult = actionResult.Result as OkObjectResult;
            var result = okResult.Value as List<TaxGroupDTO>;

            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetById_ReturnsOk_WhenTaxGroupExists()
        {
            var dto = new TaxGroupDTO { Id = 1, Name = "Standard", Vat = 10, EcoTax = 5 };
            _serviceMock.Setup(s => s.getTaxGroupById(1)).ReturnsAsync(dto);

            var actionResult = await _controller.GetById(1);
            var okResult = actionResult.Result as OkObjectResult;
            var result = okResult.Value as TaxGroupDTO;

            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal("Standard", result.Name);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenTaxGroupDoesNotExist()
        {
            _serviceMock.Setup(s => s.getTaxGroupById(999)).ReturnsAsync((TaxGroupDTO)null);

            var actionResult = await _controller.GetById(999);

            Assert.IsType<NotFoundResult>(actionResult.Result);
        }

        [Fact]
        public async Task Create_ReturnsCreatedResult_WhenValid()
        {
            var dto = new CreateTaxGroupDTO { Name = "New", Vat = 10, Eco_tax = 5 };
            var created = new TaxGroupDTO { Id = 1, Name = "New", Vat = 10, EcoTax = 5 };

            _serviceMock.Setup(s => s.CreateTaxGroup(dto)).ReturnsAsync(created);

            var actionResult = await _controller.Create(dto);
            var result = actionResult.Result as CreatedAtActionResult;
            var value = result.Value as TaxGroupDTO;

            Assert.NotNull(result);
            Assert.Equal(201, result.StatusCode);
            Assert.Equal("New", value.Name);
        }

        [Fact]
        public async Task Create_ReturnsConflict_OnArgumentException()
        {
            var dto = new CreateTaxGroupDTO { Name = "", Vat = 0, Eco_tax = 0 };
            _serviceMock.Setup(s => s.CreateTaxGroup(dto))
                .ThrowsAsync(new ArgumentException("Invalid data"));

            var actionResult = await _controller.Create(dto);
            var result = actionResult.Result as ObjectResult;

            Assert.Equal(409, result.StatusCode);
        }

        [Fact]
        public async Task UpdateTaxGroup_ReturnsOk_WhenSuccessful()
        {
            var dto = new UpdateTaxGroupDTO { Name = "Updated", Vat = 20, Eco_tax = 10 };
            var updated = new TaxGroupDTO { Id = 1, Name = "Updated", Vat = 20, EcoTax = 10 };

            _serviceMock.Setup(s => s.UpdateTaxGroup(1, dto)).ReturnsAsync(updated);

            var actionResult = await _controller.UpdateTaxGroup(1, dto);
            var result = actionResult.Result as OkObjectResult;
            var value = result.Value as TaxGroupDTO;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Updated", value.Name);
        }

        [Fact]
        public async Task UpdateTaxGroup_ReturnsNotFound_WhenNotExists()
        {
            var dto = new UpdateTaxGroupDTO { Name = "NotFound", Vat = 10, Eco_tax = 5 };
            _serviceMock.Setup(s => s.UpdateTaxGroup(999, dto)).ReturnsAsync((TaxGroupDTO)null);

            var actionResult = await _controller.UpdateTaxGroup(999, dto);
            var result = actionResult.Result as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task UpdateTaxGroup_ReturnsConflict_OnValidationError()
        {
            var dto = new UpdateTaxGroupDTO { Name = "", Vat = 10, Eco_tax = 5 };
            _serviceMock.Setup(s => s.UpdateTaxGroup(1, dto))
                .ThrowsAsync(new ArgumentException("Invalid data"));

            var actionResult = await _controller.UpdateTaxGroup(1, dto);
            var result = actionResult.Result as ObjectResult;

            Assert.Equal(409, result.StatusCode);
        }

        [Fact]
        public async Task Delete_ReturnsNoContent_WhenSuccessful()
        {
            _serviceMock.Setup(s => s.DeleteTaxGroup(1)).ReturnsAsync(true);

            var result = await _controller.Delete(1);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsConflict_WhenInUse()
        {
            _serviceMock.Setup(s => s.DeleteTaxGroup(1))
                .ThrowsAsync(new ArgumentException("In use"));

            var result = await _controller.Delete(1);
            var objectResult = result as ObjectResult;

            Assert.Equal(409, objectResult.StatusCode);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenMissing()
        {
            _serviceMock.Setup(s => s.DeleteTaxGroup(1))
                .ThrowsAsync(new Exception("not found"));

            var result = await _controller.Delete(1);
            var objectResult = result as ObjectResult;

            Assert.Equal(404, objectResult.StatusCode);
        }
    }
}
