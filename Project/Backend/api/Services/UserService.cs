using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;
using api.DTO;
using api.Interfaces;
using api.Model;
using api.Repositories;
using Microsoft.AspNetCore.DataProtection.Repositories;

namespace api.Services
{
    public class UserService:IUserService
    {
        private readonly IUserRepository _repository;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITaxGroupRepository _taxGroupRepository;
        private readonly IPlanRepository _planRepository;
        public UserService(IUserRepository repository, IHttpContextAccessor httpContextAccessor, ITaxGroupRepository taxGroupRepository, IPlanRepository planRepository)
        {
            _repository = repository;
            _httpContextAccessor=httpContextAccessor;
            _taxGroupRepository=taxGroupRepository;
            _planRepository=planRepository;
        }

        public bool IsValidEmail(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<UserDTO>> getAllUsers()
        {
            var users = await _repository.getAll();
            return users.Select(user => new UserDTO
            {
                Id = user.Id,
                Username = user.Username,
                Role = user.Role,
                Consumption = user.Consumption,
                Email = user.Email,
                Details = new UserDetailDTO
                {
                    PlanName = user.Plan?.Name,
                    PlanId = user.Plan?.Id,
                    TaxGroupName = user.TaxGroup?.Name,
                    TaxGroupId = user.TaxGroup?.Id
                }


            }).ToList();
        }

        public async Task<UserDTO?> getUserById(int id)
        {
            var loggedInId =int.Parse( _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var user = await _repository.GetById(id);
            if (user == null)
                throw new Exception("User not found.");
            if(user.Id!=loggedInId)
                throw new UnauthorizedAccessException("You can only see your own account!");
            
            return new UserDTO
            {
                Id = user.Id,
                Username = user.Username,
                Role = user.Role,
                Consumption = user.Consumption,
                Email = user.Email,
                Details = new UserDetailDTO
                {
                    PlanName = user.Plan?.Name,
                    PlanId = user.Plan?.Id,
                    TaxGroupName = user.TaxGroup?.Name,
                    TaxGroupId = user.TaxGroup?.Id
                }


            };
        }

        public async Task<UserDTO?> getUserByUsername(string username)
        {
            var user = await _repository.GetByUsername(username);
            if (user == null) return null;
            return new UserDTO
            {
                Id = user.Id,
                Username = user.Username,
                Role = user.Role,
                Consumption = user.Consumption,
                Email = user.Email,
                Details = new UserDetailDTO
                {
                    PlanName = user.Plan?.Name,
                    PlanId = user.Plan?.Id,
                    TaxGroupName = user.TaxGroup?.Name,
                    TaxGroupId = user.TaxGroup?.Id
                }


            };
        }

        public async Task<UserDTO> CreateUser(CreateUserDTO dto)
        {
            // Map the incoming DTO to your entity
            var user = new User
            {
                Username = dto.Username,
                Consumption = dto.Consumption,
                Password = dto.Password,
                Role = dto.Role,
                TaxGroupId = dto.TaxGroupId,
                Email = dto.Email
            };

            var createdUser = await _repository.CreateUser(user);

            // Map back to DTO
            return new UserDTO
            {
                Id = createdUser.Id,
                Username = createdUser.Username,
                Role = createdUser.Role,
                Consumption = createdUser.Consumption,
                Email = createdUser.Email,
                Details = new UserDetailDTO
                {
                    PlanName = createdUser.Plan?.Name,
                    PlanId = createdUser.Plan?.Id,
                    TaxGroupName = createdUser.TaxGroup?.Name,
                    TaxGroupId = createdUser.TaxGroup?.Id
                }


            };
        }
        public async Task<IEnumerable<object>> GetUserCountByTaxGroup()
        {
            return await _repository.GetUserCountByTaxGroup();
        }
        public async Task<IEnumerable<object>> GetUserCountByPlan()
        {
            return await _repository.GetUserCountByPlan();
        }
        public async Task<UserDTO?> UpdateUser(int id, UpdateUserDTO dto)
        {   

            //Ensure the user is changing his own accoutn
            
            var loggedInId =int.Parse( _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            
           //---Validation---
            var user = await _repository.GetById(id);
            if (user == null)
                throw new Exception("User not found.");
            if(user.Id!=loggedInId)
                throw new UnauthorizedAccessException("You can only change your own account!");
            
            if(dto.Username!=null && string.IsNullOrWhiteSpace(dto.Username))
                throw new ArgumentException("Username cant be blank");
             if(dto.Password!=null && string.IsNullOrWhiteSpace(dto.Password))
                throw new ArgumentException("Password cant be blank");
            if (dto.Consumption.HasValue && dto.Consumption <= 0)
                throw new ArgumentException("Consumption must be positive.");
            if(dto.Email!=null && !IsValidEmail(dto.Email))
                 throw new ArgumentException("Invalid Email");
            //Check if plan and tax group actually exist
            if (dto.TaxGroupId.HasValue)
            {
                var taxGroup = await _taxGroupRepository.GetById(dto.TaxGroupId.Value);
                if (taxGroup == null)
                    throw new ArgumentException("Tax group does not exist.");
            }

            if (dto.PlanId.HasValue)
            {
                var plan = await _planRepository.GetById(dto.PlanId.Value);
                if (plan == null)
                    throw new ArgumentException("Plan does not exist.");
            }

            var updatedUser = await _repository.UpdateUser(id, dto);
            if (updatedUser == null)
                return null;

            return new UserDTO
            {
                Id = updatedUser.Id,
                Username = updatedUser.Username,
                Role = updatedUser.Role,
                Consumption = updatedUser.Consumption,
                Email = updatedUser.Email,
                TaxGroup = updatedUser.TaxGroup == null ? null : new TaxGroupDTO
                {
                    Id = updatedUser.TaxGroup.Id,
                    Name = updatedUser.TaxGroup.Name,
                    EcoTax = updatedUser.TaxGroup.Eco_tax,
                    Vat = updatedUser.TaxGroup.Vat
                },
                Details = new UserDetailDTO
                {
                    PlanId = updatedUser.Plan?.Id ?? 0,
                    PlanName = updatedUser.Plan?.Name,
                    TaxGroupId = updatedUser.TaxGroup?.Id ?? 0,
                    TaxGroupName = updatedUser.TaxGroup?.Name
                }
            };
        }

        public async Task<bool> DeleteUser(int id)
        {
            var loggedInId =int.Parse( _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var user = await _repository.GetById(id);
            if (user == null)
                throw new Exception("User not found.");
            if(user.Id!=loggedInId)
                throw new UnauthorizedAccessException("You can only delete your own account!");
            var result = await _repository.DeleteUser(id);
            if (!result)
                throw new Exception($"User with id {id} not found.");

            return true;
        }

        
         
    }
}