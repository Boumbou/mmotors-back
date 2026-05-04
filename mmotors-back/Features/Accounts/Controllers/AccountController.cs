using System.Reflection.Metadata;
using mmotors_back.Data;
using mmotors_back.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using mmotors_back.Features.Accounts.Dtos;
using mmotors_back.Mappers;

namespace mmotors_back.Features.Accounts.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly ILogger<AccountController> _logger;

        public AccountController( UserManager<User> userManager, SignInManager<User> signInManager, UserMapper userMapper, ILogger<AccountController> logger)
        {
            _authService = new AuthService(userManager, signInManager, userMapper);
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(new { Errors = new[] { "Invalid registration data" } });
            }

            var result = await _authService.RegisterUserAsync(registerDto);
            if (result.Succeeded)
            {
                return Ok();
            }

            _logger.LogError("User registration failed: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            return BadRequest(result.Errors);
        }
    }
}