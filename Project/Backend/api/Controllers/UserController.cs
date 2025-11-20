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
{
    [ApiController]
    [Route("users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _service;

        public UserController(IUserService service)
        {
            _service = service;
        }
        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<ActionResult<UserDTO>> GetAll()
        {
            var users = await _service.getAllUsers();
            return Ok(users);
        }

        [Authorize(Roles = "user, admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDTO>> GetById(int id)
        {   
            try{
            var user = await _service.getUserById(id);
            if (user == null)
                return NotFound();
            return Ok(user);
            }
            catch(UnauthorizedAccessException ex)
            {   
                return Conflict(new { message = ex.Message }); 
            }
              catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
           
        }


        [Authorize(Roles = "user")]
        [HttpPut("{id}")]
        public async Task<ActionResult<UserDTO>> UpdateUser(int id, UpdateUserDTO dto)
        {   
            try{
            var updatedUser = await _service.UpdateUser(id, dto);
            if (updatedUser == null)
                return NotFound($"User with id {id} not found.");

            return Ok(updatedUser);
            }
            catch (ArgumentException ex)
            {   
                return Conflict(new { message = ex.Message }); 
            }
            catch(UnauthorizedAccessException ex)
            {   
                return Conflict(new { message = ex.Message }); 
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }

        }

        [Authorize(Roles ="admin, user")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteUser(id);
                return NoContent(); // 204 Success, no response body
            }
            catch(UnauthorizedAccessException ex)
            {   
                return Conflict(new { message = ex.Message }); 
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
            
        }
        [Authorize(Roles ="admin")]
        [HttpGet("count-by-taxgroup")]
        public async Task<IActionResult> GetUserCountByTaxGroup()
        {
            var result = await _service.GetUserCountByTaxGroup();
            return Ok(result);
        }
        [Authorize(Roles ="admin")]
        [HttpGet("count-by-plan")]
        public async Task<IActionResult> GetUserCountByPlan()
        {
            var result = await _service.GetUserCountByPlan();
            return Ok(result);
        }

        
    }
    
}