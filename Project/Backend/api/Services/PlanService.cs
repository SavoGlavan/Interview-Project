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
    public class PlanService:IPlanService
    {
        private readonly IPlanRepository _repository;

        private readonly ITaxGroupRepository _taxGroupRepository;
        private readonly IUserRepository _userRepository;
        public PlanService(IPlanRepository repository, ITaxGroupRepository taxGroupRepository, IUserRepository userRepository)
        {
            _repository = repository;
            _taxGroupRepository = taxGroupRepository;
            _userRepository=userRepository;
        }

        public async Task<List<PlanDTO>> getAllPlans()
        {
            var plans = await _repository.getAll();
            return plans.Select(p => new PlanDTO
            {
                Id = p.Id,
                Name = p.Name,
                Discount= p.Discount,
                Prices = p.Prices.OrderBy(p => p.Threshold ?? int.MaxValue).Select(pr => new PriceThresholdDTO
                {
                    Id = pr.Id,
                    Price = pr.Price,
                    Threshold = pr.Threshold
                }).ToList()
            }
            ).ToList();
        }

        public async Task<PlanDTO?> getPlanById(int id)
        {
            var plan = await _repository.GetById(id);
            if (plan == null) return null;
            return new PlanDTO
            {
                Id = plan.Id,
                Name = plan.Name,
                Discount=plan.Discount,
                Prices = plan.Prices.OrderBy(p => p.Threshold ?? int.MaxValue).Select(pr => new PriceThresholdDTO
                {
                    Id = pr.Id,
                    Price = pr.Price,
                    Threshold = pr.Threshold
                }).ToList()
            }
            ;
        }

        public async Task<PlanDTO> CreatePlan(CreatePlanDTO dto)
        {
            // Maping the incoming DTO entity
            var plan = new Plan
            {
                Name = dto.Name,
                Discount = dto.Discount,
                Prices = dto.Prices.OrderBy(p => p.Threshold ?? int.MaxValue).Select(p => new PriceThreshold
                {
                    Price = p.Price,
                    Threshold = p.Threshold
                }).ToList()
            };
            // -- VALIDATIONS--
            if (plan.Prices == null || !plan.Prices.Any())
                throw new ArgumentException("The plan must have at least one price tier.");

            if (plan.Prices.GroupBy(p => p.Threshold).Any(g => g.Count() > 1))
                throw new ArgumentException("Duplicate thresholds are not allowed.");

            if (plan.Prices.Any(p => p.Price <= 0))
                throw new ArgumentException("Price cannot be negative or 0.");

            if (string.IsNullOrWhiteSpace(plan.Name))
                throw new ArgumentException("Plan name cannot be empty.");
            
            if (plan.Discount < 0 || plan.Discount > 100)
                throw new ArgumentException("Discount must be between 0 and 100.");
            
            if (plan.Prices.Any(p => p.Threshold.HasValue && p.Threshold <= 0))
                throw new ArgumentException("Thresholds must be positive numbers.");

            if (plan.Prices.Count(p => p.Threshold == null) != 1)
                throw new ArgumentException("There must be an open ended threshold in the plan");
            
            var createdPlan = await _repository.CreatePlan(plan);

            // Map back to DTO
            return new PlanDTO
            {
                Id = createdPlan.Id,
                Name = createdPlan.Name,
                Discount = createdPlan.Discount,
                Prices = createdPlan.Prices.OrderBy(p => p.Threshold ?? int.MaxValue).Select(pr => new PriceThresholdDTO
                {
                    Id = pr.Id,
                    Price = pr.Price,
                    Threshold = pr.Threshold
                }).ToList()
            };
        }

        public async Task<PlanDTO?> UpdatePlan(int id, UpdatePlanDTO dto)
        {   

            //Data Validation
             if (dto.Prices == null || !dto.Prices.Any())
                throw new ArgumentException("The plan must have at least one price tier.");

            if (dto.Prices.GroupBy(p => p.Threshold).Any(g => g.Count() > 1))
                throw new ArgumentException("Duplicate thresholds are not allowed.");

            if (dto.Prices.Any(p => p.Price <= 0))
                throw new ArgumentException("Price cannot be negative or 0.");

            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Plan name cannot be empty.");
            
            if (dto.Discount < 0 || dto.Discount > 100)
                throw new ArgumentException("Discount must be between 0 and 100.");
            
            if (dto.Prices.Any(p => p.Threshold.HasValue && p.Threshold <= 0))
                throw new ArgumentException("Thresholds must be positive numbers.");

            if (dto.Prices.Count(p => p.Threshold == null) != 1)
                throw new ArgumentException("There must be an open ended threshold in the plan");

            var plan = await _repository.UpdatePlan(id, dto);
            if (plan == null)
                return null;

            return new PlanDTO
            {
                Id = plan.Id,
                Name = plan.Name,
                Discount = plan.Discount,
                Prices = plan.Prices.OrderBy(p => p.Threshold ?? int.MaxValue).Select(p => new PriceThresholdDTO
                {
                    Id = p.Id,
                    Price = p.Price,
                    Threshold = p.Threshold
                }).ToList()
            };
        }
        public async Task<bool> DeletePlan(int id)
        {
        // check if plan exists
            var plan = await _repository.GetById(id);
            if (plan == null)
                throw new Exception("Plan not found.");

            // check if any users are using this plan
            var planInUse = await _userRepository.planHasUsers(id);
            if (planInUse)
                throw new ArgumentException("Cannot delete a plan that is assigned to one or more users.");
            
            await _repository.DeletePlan(id);
            return true;
        }

        public async Task<RecomendationResponseDTO> RecommendPlan(double userConsumption, int taxGroupId)
        {
            var taxGroup = await _taxGroupRepository.GetById(taxGroupId);
            var plans = await _repository.getAll();

            Plan bestPlan = null;
            decimal lowestCost = decimal.MaxValue;

            foreach (var plan in plans)
            {
                decimal totalCost = CalculateTotalCost(plan, userConsumption, taxGroup);

                if (totalCost < lowestCost)
                {
                    lowestCost = totalCost;
                    bestPlan = plan; // map only needed info
                }
            }

            return new RecomendationResponseDTO
            {
                planDTO = new PlanDTO
                {
                    Id = bestPlan.Id,
                    Name = bestPlan.Name,
                    Discount = bestPlan.Discount,
                    Prices = bestPlan.Prices.OrderBy(p => p.Threshold ?? int.MaxValue).Select(pr => new PriceThresholdDTO
                    {
                        Id = pr.Id,
                        Price = pr.Price,
                        Threshold = pr.Threshold
                    }).ToList()
                },
                totalPrice = lowestCost


            };
        }


        public decimal CalculateTotalCost(Plan plan, double consumption, TaxGroup taxGroup)
        {
            decimal total = 0m;
            double remaining = consumption;

            // Ensure correct tier order
            var sortedTiers = plan.Prices
                .Where(t => t.Threshold.HasValue)
                .OrderBy(t => t.Threshold.Value)
                .ToList();

            var noLimitTier = plan.Prices.FirstOrDefault(t => t.Threshold == null);
            if (noLimitTier != null)
                sortedTiers.Add(noLimitTier);

            double previousThreshold = 0;

            foreach (var tier in sortedTiers)
            {
                double tierLimit = tier.Threshold ?? double.MaxValue;
                double consumptionInTier = Math.Min(remaining, tierLimit - previousThreshold);

                total += (decimal)consumptionInTier * tier.Price;

                remaining -= consumptionInTier;
                previousThreshold = tierLimit;

                if (remaining <= 0)
                    break;
            }

            // Apply plan discount
            if (plan.Discount > 0)
                total *= 1 - (decimal)plan.Discount/100; // remove /100

            // Apply VAT and eco tax
            total *= 1 + (decimal)(taxGroup.Vat + taxGroup.Eco_tax)/100;

            return total;
        }



    }
}