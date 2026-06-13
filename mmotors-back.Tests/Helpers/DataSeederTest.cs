using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using mmotors_back.Data;
using mmotors_back.Models;
using Moq;

namespace mmotors_back.Tests.Helpers;

public class DataSeederTest
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;
    private readonly DataSeeder _dataSeeder;

    public DataSeederTest()
    {
        _userManagerMock = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(), null!, null!, null!, null!, null!, null!, null!, null!);
        _roleManagerMock = new Mock<RoleManager<IdentityRole>>(
            Mock.Of<IRoleStore<IdentityRole>>(), null!, null!, null!, null!);
        _dataSeeder = new DataSeeder(_userManagerMock.Object, _roleManagerMock.Object);
    }

    #region SeedData
    [Fact]
    public async Task SeedData_ShouldCreateUserAndAssignRole_WhenRoleExistsAndUserDoesNotExist()
    {
        // Arrange
        var adminUser = CreateAdminUser();
        _roleManagerMock.Setup(roleManager => roleManager.RoleExistsAsync("Admin")).ReturnsAsync(true);
        _userManagerMock.Setup(userManager => userManager.FindByEmailAsync(adminUser.Email!)).ReturnsAsync((User?)null);
        _userManagerMock.Setup(userManager => userManager.CreateAsync(adminUser, "adminPassword")).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(userManager => userManager.AddToRoleAsync(adminUser, "Admin")).ReturnsAsync(IdentityResult.Success);

        // Act
        await _dataSeeder.SeedData(adminUser, "adminPassword", "Admin");

        // Assert
        _userManagerMock.Verify(userManager => userManager.CreateAsync(adminUser, "adminPassword"), Times.Once);
        _userManagerMock.Verify(userManager => userManager.AddToRoleAsync(adminUser, "Admin"), Times.Once);
    }

    [Fact]
    public async Task SeedData_ShouldReturnWithoutCreatingUser_WhenExistingUserAlreadyHasRole()
    {
        // Arrange
        var adminUser = CreateAdminUser();
        _roleManagerMock.Setup(roleManager => roleManager.RoleExistsAsync("Admin")).ReturnsAsync(true);
        _userManagerMock.Setup(userManager => userManager.FindByEmailAsync(adminUser.Email!)).ReturnsAsync(adminUser);
        _userManagerMock.Setup(userManager => userManager.IsInRoleAsync(adminUser, "Admin")).ReturnsAsync(true);

        // Act
        await _dataSeeder.SeedData(adminUser, "adminPassword", "Admin");

        // Assert
        _userManagerMock.Verify(userManager => userManager.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        _userManagerMock.Verify(userManager => userManager.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SeedData_ShouldThrowException_WhenRoleDoesNotExist()
    {
        // Arrange
        var adminUser = CreateAdminUser();
        _roleManagerMock.Setup(roleManager => roleManager.RoleExistsAsync("Admin")).ReturnsAsync(false);

        // Act
        var action = () => _dataSeeder.SeedData(adminUser, "adminPassword", "Admin");

        // Assert
        var exception = await Assert.ThrowsAsync<Exception>(action);
        Assert.Equal("Admin role does not exist", exception.Message);
    }

    [Fact]
    public async Task SeedData_ShouldThrowException_WhenAdminEmailIsMissing()
    {
        // Arrange
        var adminUser = CreateAdminUser();
        adminUser.Email = null;
        _roleManagerMock.Setup(roleManager => roleManager.RoleExistsAsync("Admin")).ReturnsAsync(true);

        // Act
        var action = () => _dataSeeder.SeedData(adminUser, "adminPassword", "Admin");

        // Assert
        var exception = await Assert.ThrowsAsync<Exception>(action);
        Assert.Equal("Admin user or password is null or empty", exception.Message);
    }

    [Fact]
    public async Task SeedData_ShouldThrowException_WhenUserCreationFails()
    {
        // Arrange
        var adminUser = CreateAdminUser();
        _roleManagerMock.Setup(roleManager => roleManager.RoleExistsAsync("Admin")).ReturnsAsync(true);
        _userManagerMock.Setup(userManager => userManager.FindByEmailAsync(adminUser.Email!)).ReturnsAsync((User?)null);
        _userManagerMock.Setup(userManager => userManager.CreateAsync(adminUser, "adminPassword"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "create failed" }));

        // Act
        var action = () => _dataSeeder.SeedData(adminUser, "adminPassword", "Admin");

        // Assert
        var exception = await Assert.ThrowsAsync<Exception>(action);
        Assert.Equal("Failed to create admin user", exception.Message);
    }

    [Fact]
    public async Task SeedData_ShouldThrowException_WhenAddToRoleFails()
    {
        // Arrange
        var adminUser = CreateAdminUser();
        _roleManagerMock.Setup(roleManager => roleManager.RoleExistsAsync("Admin")).ReturnsAsync(true);
        _userManagerMock.Setup(userManager => userManager.FindByEmailAsync(adminUser.Email!)).ReturnsAsync((User?)null);
        _userManagerMock.Setup(userManager => userManager.CreateAsync(adminUser, "adminPassword")).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(userManager => userManager.AddToRoleAsync(adminUser, "Admin"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "add role failed" }));

        // Act
        var action = () => _dataSeeder.SeedData(adminUser, "adminPassword", "Admin");

        // Assert
        var exception = await Assert.ThrowsAsync<Exception>(action);
        Assert.Equal("Failed to add admin user to Admin role", exception.Message);
    }
    #endregion

    #region SeedVehicles
    [Fact]
    public async Task SeedVehicles_ShouldAddDefaultVehicles_WhenDatabaseIsEmpty()
    {
        // Arrange
        await using var dbContext = CreateDbContext();

        // Act
        await _dataSeeder.SeedVehicles(dbContext);

        // Assert
        Assert.Equal(3, dbContext.Vehicles.Count());
        Assert.Contains(dbContext.Vehicles, vehicle => vehicle.Brand == "Toyota" && vehicle.Model == "Corolla");
        Assert.Contains(dbContext.Vehicles, vehicle => vehicle.Brand == "Tesla" && vehicle.Model == "Model 3");
        Assert.Contains(dbContext.Vehicles, vehicle => vehicle.Brand == "Renault" && vehicle.Model == "Clio");
    }

    [Fact]
    public async Task SeedVehicles_ShouldNotAddVehicles_WhenDatabaseAlreadyContainsVehicles()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        dbContext.Vehicles.Add(new Vehicle
        {
            Brand = "Existing",
            Model = "Vehicle",
            Motorization = Motorization.Hybrid,
            Mileage = 1000,
            ListedAmount = 10000,
            Year = 2024,
            ListingType = ListingType.SALE,
            Status = VehicleStatus.AVAILABLE,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync();

        // Act
        await _dataSeeder.SeedVehicles(dbContext);

        // Assert
        Assert.Single(dbContext.Vehicles);
        Assert.Equal("Existing", dbContext.Vehicles.Single().Brand);
    }
    #endregion

    #region SeedDocumentTemplates
    [Fact]
    public async Task SeedDocumentTemplates_ShouldAddDefaultTemplates_WhenDatabaseIsEmpty()
    {
        // Arrange
        await using var dbContext = CreateDbContext();

        // Act
        await _dataSeeder.SeedDocumentTemplates(dbContext);

        // Assert
        Assert.Equal(4, dbContext.DocumentTemplates.Count());
        Assert.Contains(dbContext.DocumentTemplates, template => template.Id == 1 && template.Name == "Justificatif d'identité");
        Assert.Contains(dbContext.DocumentTemplates, template => template.Id == 4 && template.Name == "Permis de conduire");
    }

    [Fact]
    public async Task SeedDocumentTemplates_ShouldNotAddTemplates_WhenDatabaseAlreadyContainsTemplates()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        dbContext.DocumentTemplates.Add(new DocumentTemplate
        {
            Id = 99,
            Name = "Existing template",
            Type = DocumentType.COMMON_APPLICATION
        });
        await dbContext.SaveChangesAsync();

        // Act
        await _dataSeeder.SeedDocumentTemplates(dbContext);

        // Assert
        Assert.Single(dbContext.DocumentTemplates);
        Assert.Equal("Existing template", dbContext.DocumentTemplates.Single().Name);
    }
    #endregion

    private static User CreateAdminUser()
    {
        return new User
        {
            Name = "Admin",
            LastName = "User",
            UserName = "admin@example.com",
            Email = "admin@example.com"
        };
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}