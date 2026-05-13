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
        private readonly SignInManager<User> _signInManager;
        private readonly UserMapper _userMapper;
        private readonly ITokenService _tokenService;

        public AuthService(UserManager<User> userManager, SignInManager<User> signInManager, UserMapper userMapper, ITokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _userMapper = userMapper;
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
                
                string token = _tokenService.GenerateToken(user);
                
                return new RegisterResultDto { Result = result, User = UserMapper.ToDTO(user), Token = token };
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

            SignInResult result = await _signInManager.PasswordSignInAsync(user, loginDto.Password, false, false);
            
            if (!result.Succeeded)
            {
                return new LoginResultDto { Result = result };
            }

            string userToken = _tokenService.GenerateToken(user);

            //return the result including signin result and token if successful
            return new LoginResultDto
            {
                Result = result,
                User = UserMapper.ToDTO(user),
                Token = userToken
            };

        }
}