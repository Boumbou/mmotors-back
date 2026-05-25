/*
    * this file is used as a wrapper to seed the database with initial data. It is called in the Program.cs file.
*/

using mmotors_back.Data;
using mmotors_back.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.VisualBasic;


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

    public  async Task SeedData(User adminUser, string adminPassword, string roleName)
    {
        //check if role exists, if not throw an exception
        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            throw new Exception($"{roleName} role does not exist");
        }
        if(adminUser.Email == null || string.IsNullOrEmpty(adminPassword))
        {
            throw new Exception("Admin user or password is null or empty");
        }
        //check if user exists and has role admin, if not create it and add it to the role
        User? existingAdminUser = await _userManager.FindByEmailAsync(adminUser.Email);

        if (existingAdminUser != null)        {
            if (await _userManager.IsInRoleAsync(existingAdminUser, roleName))
            {
                return;
            }
        }
        
        IdentityResult newAdminUser = _userManager.CreateAsync(adminUser, adminPassword).Result;

        if (!newAdminUser.Succeeded)
        {
            throw new Exception("Failed to create admin user");
        }
        if(!(await _userManager.AddToRoleAsync(adminUser, roleName)).Succeeded)
        {
            throw new Exception($"Failed to add admin user to {roleName} role");
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
                Motorization = Motorization.Petrol,
                Mileage = 50000,
                ListedAmount = 15000,
                Year = 2019,
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
                Motorization = Motorization.Electric,
                Mileage = 20000,
                Year = 2020,
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
                Year = 2018,
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

    public async Task SeedDocumentTemplates(AppDbContext dbContext)
    {
        if (dbContext.DocumentTemplates.Any())
        {
            return; // Data already seeded
        }

        var documentTemplates = new List<DocumentTemplate>
        {
            //seed initial document templates
            new DocumentTemplate
            {
                Id = 1,
                Name = "Justificatif d'identité",
                Type = DocumentType.COMMON_APPLICATION
            },
            new DocumentTemplate
            {
                Id = 2,
                Name = "Justificatif de domicile",
                Type = DocumentType.COMMON_APPLICATION
            },
            new DocumentTemplate
            {
                Id = 3,
                Name = "RIB",
                Type = DocumentType.RENTAL_APPLICATION
            },
            new DocumentTemplate
            {
                Id = 4,
                Name = "Permis de conduire",
                Type = DocumentType.RENTAL_APPLICATION
            }

        };

        dbContext.DocumentTemplates.AddRange(documentTemplates);
        await dbContext.SaveChangesAsync();
    }
}