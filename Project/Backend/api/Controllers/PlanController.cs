using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.DTO;
using api.Model;
using api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using api.Interfaces;

namespace api.Controllers
{
    [Route("plan")]
    [ApiController]
    public class PlanController : ControllerBase
    {

        private readonly IPlanService _service;

        public PlanController(IPlanService service)
        {
            _service = service;
        }
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<PlanDTO>> GetAll()
        {
            var users = await _service.getAllPlans();
            return Ok(users);
        }
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<PlanDTO>> GetById(int id)
        {
            var plan = await _service.getPlanById(id);
            if (plan == null)
                return NotFound();
            return Ok(plan);
        }
        [Authorize(Roles ="user")]
        [HttpPost("recommend")]
        public async Task<ActionResult<RecomendationResponseDTO>> RecommendPlan([FromBody] RecommendationRequestDTO request)
        {
            var recommendedPlan = await _service.RecommendPlan(request.Consumption, request.TaxGroupId);
            return Ok(recommendedPlan);
        }

        [Authorize(Roles ="admin")]
        [HttpPost]
        public async Task<ActionResult<PlanDTO>> Create([FromBody] CreatePlanDTO dto)
        {    
            try{
                var createdPlan = await _service.CreatePlan(dto);
                return CreatedAtAction(nameof(GetById), new { id = createdPlan.Id }, createdPlan);
            }
            catch (ArgumentException ex)
            {   
                //Plan cant be deleted cause its in use
                return Conflict(new { message = ex.Message }); 
            }
            catch (Exception ex)
            {
                // Plan doesnt exist / invalid id
                return StatusCode(404, new { message = "Error creating plan!", detail = ex.Message });
            }
            
        }
        [Authorize(Roles = "admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<PlanDTO>> UpdatePlan(int id, [FromBody] UpdatePlanDTO dto)
        {   
            try{
                var updated = await _service.UpdatePlan(id, dto);
                if (updated == null)
                    return NotFound();
                
                return Ok(updated);
            }
            catch (ArgumentException ex)
            {   
                //Plan cant be deleted cause its in use
                return Conflict(new { message = ex.Message }); 
            }
            catch (Exception ex)
            {
                // Plan doesnt exist / invalid id
                return StatusCode(404, new { message = "Plan not Found", detail = ex.Message });
            }
        }
        [Authorize(Roles ="admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeletePlan(id);
                return NoContent();
            }
            catch (ArgumentException ex)
            {   
                //Plan cant be deleted cause its in use
                return Conflict(new { message = ex.Message }); 
            }
            
            catch (Exception ex)
            {
                // Plan doesnt exist / invalid id
                return StatusCode(404, new { message = "Plan not Found", detail = ex.Message });
            }
        }
    }
}