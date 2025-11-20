using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.DTO;
using api.Interfaces;
using api.Model;
using api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{   [ApiController]
    [Route("taxgroups")]
    public class TaxGroupController:ControllerBase
    {
    

        private readonly ITaxGroupService _service;

        public TaxGroupController(ITaxGroupService service)
        {
            _service = service;
        }
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<TaxGroupDTO>> GetAll()
        {
            var users = await _service.getAllTaxGroups();
            return Ok(users);
        }
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<TaxGroupDTO>> GetById(int id)
        {
            var taxGroup = await _service.getTaxGroupById(id);
            if (taxGroup == null)
                return NotFound();
            return Ok(taxGroup);
        }
        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<ActionResult<TaxGroupDTO>> Create([FromBody] CreateTaxGroupDTO dto)
        {
           
            try
            {
                var createdTaxGroup = await _service.CreateTaxGroup(dto);
                return CreatedAtAction(nameof(GetById), new { id = createdTaxGroup.Id }, createdTaxGroup);
            }
            catch (ArgumentException ex)
            {   
                
                return Conflict(new { message = ex.Message }); 
            }
            catch (Exception ex)
            {
                
                return StatusCode(404, new { message = "Error creating tax group!", detail = ex.Message });
            }
            
        }

        [Authorize(Roles = "admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<TaxGroupDTO>> UpdateTaxGroup(int id, UpdateTaxGroupDTO dto)
        {   
            try{
                var updatedTaxGroup = await _service.UpdateTaxGroup(id, dto);
                if (updatedTaxGroup == null)
                    return NotFound($"TaxGroup with id {id} not found.");

                return Ok(updatedTaxGroup);
            }
            catch (ArgumentException ex)
            {   
                
                return Conflict(new { message = ex.Message }); 
            }
            catch (Exception ex)
            {
                
                return StatusCode(404, new { message = "Error updating tax group!", detail = ex.Message });
            }
            
        }
        
        [Authorize(Roles ="admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteTaxGroup(id);
                return NoContent(); // 204 Success, no response body
            }
            catch (ArgumentException ex)
            {   
                //Tax Group cant be deleted cause its in use
                return Conflict(new { message = ex.Message }); 
            }
            catch (Exception ex)
            {
                return StatusCode(404, new { message = "Tax Group not found", detail = ex.Message });
            }
        }
       
        

    }
    
}