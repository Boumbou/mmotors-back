/**
 * AuthService.cs
 * Service class for handling authentication and authorization related operations.
 * This includes user registration, login, role management, and token generation.
 * It interacts with the UserManager and RoleManager from ASP.NET Identity to manage users and roles.
 * It also uses JWT for generating authentication tokens for users.
 */
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using mmotors_back.Data;
using mmotors_back.Models;
using mmotors_back.Mappers;
using mmotors_back.Features.Accounts.Dtos;
using mmotors_back.Features.Accounts.Interfaces;
using System.Data;



namespace mmotors_back.Features.Accounts.Services;
 public class AuthService
 {
        // This class will contain methods for:
        // - Registering a new user
        // - Logging in a user and generating a JWT token
        // - Managing user roles (assigning roles to users, checking user roles)
        // - Handling password resets and other authentication-related operations


        private readonly UserManager<User> _userManager;
        private readonly UserMapper _userMapper;
        private readonly ITokenService _tokenService;
        private readonly IHttpContextAccessor _httpContext;
        private readonly IWebHostEnvironment _env;

        public AuthService(UserManager<User> userManager, UserMapper userMapper, ITokenService tokenService, IHttpContextAccessor httpContext, IWebHostEnvironment env   )
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _userMapper = userMapper;
            _httpContext = httpContext;
            _env = env;
        }

        public async Task<RegisterResultDto> RegisterUserAsync(RegisterDto registerDto)
        {
            if(string.IsNullOrWhiteSpace(registerDto.Email))
            {
                return new RegisterResultDto { Result = IdentityResult.Failed(new IdentityError { Description = "Email is required" }), Token = null! };
            }
            if(string.IsNullOrWhiteSpace(registerDto.Password))
            {
                // throw exception instead of returning result
                return new RegisterResultDto { Result = IdentityResult.Failed(new IdentityError { Description = "Password is required" }), Token = null! };
            }
            
            if( await _userManager.FindByEmailAsync(registerDto.Email) != null)
            {
                return new RegisterResultDto { Result = IdentityResult.Failed(new IdentityError { Description = "Email already exists" }), Token = null! };
            }

            try
            {
                User user = UserMapper.RegisterDtoToEntity(registerDto);
                IdentityResult result = await _userManager.CreateAsync(user, registerDto.Password);

                if (result.Errors.Any())
                {
                    throw new Exception(string.Join(", ", result.Errors));
                }
                
                await _userManager.AddToRoleAsync(user, "Customer");
                
                List<string> roles = new List<string> { "Customer" };
                string token = _tokenService.GenerateToken(user, roles);
                
                return new RegisterResultDto { Result = result, User = UserMapper.ToDTO(user), Roles = roles };
            }
            catch (Exception ex)
            {
                return new RegisterResultDto { Result = IdentityResult.Failed(new IdentityError { Description = ex.Message }) };
            }
        }

        public async Task<LoginResultDto> LoginUserAsync(LoginDto loginDto)
        {
            
            User? user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                return new LoginResultDto { Result = SignInResult.Failed  };
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);

            if (!isPasswordValid)
            {
                return new LoginResultDto { Result = SignInResult.Failed };
            }

            var roles = await _userManager.GetRolesAsync(user);
            string userToken = _tokenService.GenerateToken(user, roles);

            //send the token to the client in cookies

            _httpContext?.HttpContext?.Response.Cookies.Append("token", userToken, 
                new CookieOptions { 
                    HttpOnly = true, 
                    Secure = _env.IsProduction(), // Set to true in production
                    SameSite = _env.IsProduction() ? SameSiteMode.None : SameSiteMode.Lax, // Adjust as needed
                    Expires = DateTimeOffset.UtcNow.AddHours(1),
                    Path = "/"         
                }
            );

            //return the result including signin result and token if successful
            return new LoginResultDto
            {
                Result = SignInResult.Success,
                User = UserMapper.ToDTO(user),
                Roles = roles
            };

        }
}