/*
    * this file is used as a wrapper to seed the database with initial data. It is called in the Program.cs file.
*/

using mmotors_back.Data;
using mmotors_back.Models;
using Microsoft.AspNetCore.Identity;


namespace mmotors_back.Data;
public class DataSeeder
{
    private  readonly UserManager<User> _userManager;
    private  readonly RoleManager<IdentityRole> _roleManager;

    public DataSeeder(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public  async Task SeedData(User adminUser, string adminPassword)
    {
        //check if role exists, if not throw an exception
        if (!await _roleManager.RoleExistsAsync("Admin"))
        {
            throw new Exception("Admin role does not exist");
        }
        
        //check if user exists and has role admin, if not create it and add it to the role
        User? existingAdminUser = await _userManager.FindByEmailAsync(adminUser.Email);

        if (existingAdminUser != null)        {
            if (await _userManager.IsInRoleAsync(existingAdminUser, "Admin"))
            {
                return;
            }
        }
        
        IdentityResult newAdminUser = _userManager.CreateAsync(adminUser, adminPassword).Result;

        if (!newAdminUser.Succeeded)
        {
            throw new Exception("Failed to create admin user");
        }
        if(!(await _userManager.AddToRoleAsync(adminUser, "Admin")).Succeeded)
        {
            throw new Exception("Failed to add admin user to role");
        }

        return ;
    }
}