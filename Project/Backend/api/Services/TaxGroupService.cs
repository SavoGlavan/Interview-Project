using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.DTO;
using api.Interfaces;
using api.Model;
using api.Repositories;

namespace api.Services
{
    public class TaxGroupService:ITaxGroupService
    {
        private readonly ITaxGroupRepository _repository;
        private readonly IUserRepository _userRepository;
        public TaxGroupService(ITaxGroupRepository repository, IUserRepository userRepository)
        {
            _repository = repository;
            _userRepository=userRepository;
        }

        public async Task<List<TaxGroupDTO>> getAllTaxGroups()
        {
            var taxGroups = await _repository.getAll();
            return taxGroups.Select(taxGroup => new TaxGroupDTO
            {
               Id = taxGroup.Id,
                Name = taxGroup.Name,
                Vat = taxGroup.Vat,
                EcoTax = taxGroup.Eco_tax
            }
            ).ToList();
        }

        public async Task<TaxGroupDTO?> getTaxGroupById(int id)
        {
            var taxGroup = await _repository.GetById(id);
            if (taxGroup == null) return null;
            return new TaxGroupDTO
            {
                Id = taxGroup.Id,
                Name = taxGroup.Name,
                Vat = taxGroup.Vat,
                EcoTax = taxGroup.Eco_tax

            };
        }

        public async Task<TaxGroupDTO> CreateTaxGroup(CreateTaxGroupDTO dto)
        {   
            //---Validation---
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Plan name cannot be empty.");
            // Ensure VAT is not null and positive
            if (dto.Vat <= 0)
                throw new ArgumentException("VAT must be a positive number.");

            // Ensure EcoTax is not null and positive
            if (dto.Eco_tax <= 0)
                throw new ArgumentException("Eco tax must be a positive number.");
            // Map the incoming DTO to your entity
            var taxGroup = new TaxGroup
            {
                Name = dto.Name,
                Vat = dto.Vat,
                Eco_tax = dto.Eco_tax
            };

            var createdTaxGroup = await _repository.CreateTaxGroup(taxGroup);

            // Map back to DTO
            return new TaxGroupDTO
            {
                Id = createdTaxGroup.Id,
                Name = createdTaxGroup.Name,
                EcoTax = createdTaxGroup.Eco_tax,
                Vat = createdTaxGroup.Vat,
            };
        }

        public async Task<TaxGroupDTO?> UpdateTaxGroup(int id, UpdateTaxGroupDTO dto)
        {   

            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Tax group name cannot be empty.");
            // Ensure VAT is not null and positive
            if (dto.Vat!=null && dto.Vat <= 0)
                throw new ArgumentException("VAT must be a positive number.");

            // Ensure EcoTax is not null and positive
            if (dto.Eco_tax!=null && dto.Eco_tax <= 0)
                throw new ArgumentException("Eco tax must be a positive number.");
            var updatedTaxGroup = await _repository.UpdateTaxGroup(id, dto);
            if (updatedTaxGroup == null)
                return null;

            return new TaxGroupDTO
            {
                Name = updatedTaxGroup.Name,
                Vat = updatedTaxGroup.Vat,
                EcoTax = updatedTaxGroup.Eco_tax
            };
        }
        
        public async Task<bool> DeleteTaxGroup(int id)
        {

            var taxGroup = await _repository.GetById(id);
            if (taxGroup==null)
                throw new Exception($"TaxGroup with id {id} not found.");
            var taxGroupInUse = await _userRepository.taxGroupHasUsers(id);
            //check if tax grop is used
            if(taxGroupInUse)
                throw new ArgumentException("Cannot delete a tax group that is assigned to one or more users.");
            await _repository.DeleteTaxGroup(id);
            return true;
        }

    }
}