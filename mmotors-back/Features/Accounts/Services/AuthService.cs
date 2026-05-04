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

        public AuthService(UserManager<User> userManager, SignInManager<User> signInManager, UserMapper userMapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _userMapper = userMapper;
        }

        public async Task<IdentityResult> RegisterUserAsync(RegisterDto registerDto)
        {
            try
            {
                

                var user = UserMapper.RegisterDtoToEntity(registerDto);
                var result = await _userManager.CreateAsync(user, registerDto.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Customer");
                }

                return result;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = ex.Message });
            }
        }
}