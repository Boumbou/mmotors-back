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

    public async Task SeedVehicles(AppDbContext dbContext)
    {
        if (dbContext.Vehicles.Any())
        {
            return; // Data already seeded
        }

        var vehicles = new List<Vehicle>
        {
            new Vehicle
            {
                Brand = "Toyota",
                Model = "Corolla",
                Motorization = Motorization.Essence,
                Mileage = 50000,
                ListedAmount = 15000,
                RentalTermMonths = null,
                ListingType = ListingType.SALE,
                Status = VehicleStatus.AVAILABLE,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Vehicle
            {
                Brand = "Tesla",
                Model = "Model 3",
                Motorization = Motorization.Électrique,
                Mileage = 20000,
                ListedAmount = 35000,
                RentalTermMonths = null,
                ListingType = ListingType.SALE,
                Status = VehicleStatus.AVAILABLE,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Vehicle
            {
                Brand = "Renault",
                Model = "Clio",
                Motorization = Motorization.Diesel,
                Mileage = 80000,
                ListedAmount = 10000,
                RentalTermMonths = null,
                ListingType = ListingType.SALE,
                Status = VehicleStatus.AVAILABLE,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        dbContext.Vehicles.AddRange(vehicles);
        await dbContext.SaveChangesAsync();
    }
}