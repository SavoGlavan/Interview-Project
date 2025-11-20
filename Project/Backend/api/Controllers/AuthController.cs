using Microsoft.AspNetCore.Mvc;
using api.Services;
using api.DTO;
using Microsoft.AspNetCore.Authorization;
using api.Interfaces;


namespace api.Controllers
{   
    [AllowAnonymous]
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO loginRequest)
        {
            var token = await _authService.AuthenticateAsync(loginRequest);
            if (token == null)
                return Unauthorized(new { message = "Invalid credentials" });

            return Ok(new { token });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO registerRequest)
        {
            try
            {
                var user = await _authService.RegisterAsync(registerRequest);
                return Ok(new { message = "User registered successfully", user.Username });
            }
            catch (ArgumentException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }
    }
}
