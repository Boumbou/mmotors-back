using System.Reflection.Metadata;
using mmotors_back.Data;
using mmotors_back.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using mmotors_back.Features.Accounts.Dtos;
using mmotors_back.Mappers;
using mmotors_back.Features.Accounts.Services;
using mmotors_back.Features.Accounts.Interfaces;

namespace mmotors_back.Features.Accounts.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly ILogger<AccountController> _logger;
        private readonly IWebHostEnvironment _env;

        public AccountController( UserManager<User> userManager, UserMapper userMapper, ILogger<AccountController> logger, ITokenService tokenService, IWebHostEnvironment env)
        {
            _authService = new AuthService(userManager, userMapper, tokenService, new HttpContextAccessor(), env);
            _logger = logger;
            _env = env;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(new { Errors = new[] { "Invalid registration data" } });
            }

            var result = await _authService.RegisterUserAsync(registerDto);
            if (result.Result.Succeeded)
            {
                //return Ok(new { Token = result.Token });
                return Ok(new{
                    Result= result.Result, 
                    User= result.User, 
                    Roles= result.Roles});
            }

            _logger.LogError("User registration failed: {Errors}", string.Join(", ", result.Result.Errors.Select(e => e.Description)));
            return BadRequest(result.Result.Errors);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Errors = new[] { "Invalid login data" } });
            }

            var result = await _authService.LoginUserAsync(loginDto);
            if (result.Result.Succeeded)
            {
                return Ok(new { Result = result.Result, User = result.User, Roles = result.Roles });
            }

            _logger.LogError("User login failed: {Errors}", "Invalid email or password");
            return BadRequest(new { Errors = new[] { "Invalid email or password" } });
        }
    
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            if (_env.IsProduction())
            {
                Response.Cookies.Delete("token", new CookieOptions
                {
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Path = "/"
                });
            }
            else
            {
                Response.Cookies.Delete("token", new CookieOptions
                {
                    Secure = false,
                    SameSite = SameSiteMode.Lax,
                    Path = "/"
                });
            }

            return Ok();
        }
    }
}